/*
** $Id: luac.c,v 1.54 2006/06/02 17:37:11 lhf Exp $
** Lua compiler (saves bytecodes to files; also list bytecodes)
** See Copyright Notice in lua.h
*/

#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define luac_c
#define LUA_CORE

#include "lua.h"
#include "lauxlib.h"

#include "ldo.h"
#include "lfunc.h"
#include "lmem.h"
#include "lobject.h"
#include "lopcodes.h"
#include "lstring.h"
#include "lundump.h"

#define PROGNAME	"luac"		/* default program name */
#define	OUTPUT		PROGNAME ".out"	/* default output file */

static int listing=0;			/* list bytecodes? */
static int dumping=1;			/* dump bytecodes? */
static int stripping=0;			/* strip debug information? */
static char Output[]={ OUTPUT };	/* default output file name */
static const char* output=Output;	/* actual output file name */
static const char* progname=PROGNAME;	/* actual program name */
static int showdumpconfiguration = 0;	/* whether to show dump configuration */

static LuaDumpConfig config;

static void fatal(const char* message)
{
 fprintf(stderr,"%s: %s\n",progname,message);
 exit(EXIT_FAILURE);
}

static void cannot(const char* what)
{
 fprintf(stderr,"%s: cannot %s %s: %s\n",progname,what,output,strerror(errno));
 exit(EXIT_FAILURE);
}

static void usage(const char* message)
{
 if (*message=='-')
  fprintf(stderr,"%s: unrecognized option " LUA_QS "\n",progname,message);
 else
  fprintf(stderr,"%s: %s\n",progname,message);
 fprintf(stderr,
 "usage: %s [options] [filenames].\n"
 "Available options are:\n"
 "  -                    process stdin\n"
 "  -l                   list\n"
 "  -o name              output to file " LUA_QL("name") " (default is \"%s\")\n"
 "  -p                   parse only\n"
 "  -s                   strip debug information\n"
 "  -v                   show version information\n"
 "  -endianness " LUA_QL("value") " (default is \"1\" (0 = BE, 1 = LE)\n"
 "  -sizeof_int " LUA_QL("value") " (default is \"4\")\n"
 "  -sizeof_size_t " LUA_QL("value") " (default is \"4\")\n"
 "  -sizeof_lua_Number " LUA_QL("value") " (default is \"4\")\n"
 "  -showdumpconfig      show dump configuration values\n"
 "  --                   stop handling options\n",
 progname,Output);
 exit(EXIT_FAILURE);
}

static void ValidateConfig()
{
	if ((config.endianness < 0) || (config.endianness > 1))
		fatal("endianness must be \"0\" (BE) or \"1\" (LE)");

	if ((config.sizeof_int != 4) && (config.sizeof_int != 8))
		fatal("sizeof_int must be either \"4\" or \"8\"");
	if ((config.sizeof_size_t != 4) && (config.sizeof_size_t != 8))
		fatal("sizeof_size_t must be either \"4\" or \"8\"");
	if ((config.sizeof_lua_Number != 4) && (config.sizeof_lua_Number != 8))
		fatal("sizeof_lua_Number must be either \"4\" or \"8\"");

	if (showdumpconfiguration == 0)
		return;

	printf("\nDump configuration:\n");
	printf("-------------------\n");
	printf("  endianness:         %i (%s)\n", config.endianness, config.endianness == 0 ? "BE" : "LE");
	printf("  sizeof(int):        %i\n", config.sizeof_int);
	printf("  sizeof(size_t):     %i\n", config.sizeof_size_t);
	printf("  sizeof(lua_Number): %i\n\n", config.sizeof_lua_Number);
}

#define	IS(s)	(strcmp(argv[i],s)==0)

static int doargs(int argc, char* argv[])
{
 int i;
 int version=0;

 // Set defaults in case non are supplied
 config.endianness = 1;
 config.sizeof_int = 4;
 config.sizeof_size_t = 4;
 config.sizeof_lua_Number = 4;

 if (argv[0]!=NULL && *argv[0]!=0) progname=argv[0];
 for (i=1; i<argc; i++)
 {
  if (*argv[i]!='-')			/* end of options; keep it */
   break;
  else if (IS("--"))			/* end of options; skip it */
  {
   ++i;
   if (version) ++version;
   break;
  }
  else if (IS("-"))			/* end of options; use stdin */
   break;
  else if (IS("-l"))			/* list */
   ++listing;
  else if (IS("-endianness"))
	  config.endianness = atoi(argv[++i]);
  else if (IS("-sizeof_int"))
	  config.sizeof_int = atoi(argv[++i]);
  else if (IS("-sizeof_size_t"))
	  config.sizeof_size_t = atoi(argv[++i]);
  else if (IS("-sizeof_lua_Number"))
	  config.sizeof_lua_Number = atoi(argv[++i]);
  else if (IS("-showdumpconfig"))
	  showdumpconfiguration = 1;
  else if (IS("-o"))			/* output file */
  {
   output=argv[++i];
   if (output==NULL || *output==0) usage(LUA_QL("-o") " needs argument");
   if (IS("-")) output=NULL;
  }
  else if (IS("-p"))			/* parse only */
   dumping=0;
  else if (IS("-s"))			/* strip debug information */
   stripping=1;
  else if (IS("-v"))			/* show version */
   ++version;
  else					/* unknown option */
   usage(argv[i]);
 }
 if (i==argc && (listing || !dumping))
 {
  dumping=0;
  argv[--i]=Output;
 }
 if (version)
 {
  printf("%s  %s\n",LUA_RELEASE,LUA_COPYRIGHT);
  if (version==argc-1) exit(EXIT_SUCCESS);
 }
 ValidateConfig();
 return i;
}

#define toproto(L,i) (clvalue(L->top+(i))->l.p)

static const Proto* combine(lua_State* L, int n)
{
 if (n==1)
  return toproto(L,-1);
 else
 {
  int i,pc;
  Proto* f=luaF_newproto(L);
  setptvalue2s(L,L->top,f); incr_top(L);
  f->source=luaS_newliteral(L,"=(" PROGNAME ")");
  f->maxstacksize=1;
  pc=2*n+1;
  f->code=luaM_newvector(L,pc,Instruction);
  f->sizecode=pc;
  f->p=luaM_newvector(L,n,Proto*);
  f->sizep=n;
  pc=0;
  for (i=0; i<n; i++)
  {
   f->p[i]=toproto(L,i-n-1);
   f->code[pc++]=CREATE_ABx(OP_CLOSURE,0,i);
   f->code[pc++]=CREATE_ABC(OP_CALL,0,1,1);
  }
  f->code[pc++]=CREATE_ABC(OP_RETURN,0,1,0);
  return f;
 }
}

static int writer(lua_State* L, const void* p, size_t size, void* u)
{
 UNUSED(L);
 return (fwrite(p,size,1,(FILE*)u)!=1) && (size!=0);
}

struct Smain {
 int argc;
 char** argv;
};

static int pmain(lua_State* L)
{
 struct Smain* s = (struct Smain*)lua_touserdata(L, 1);
 int argc=s->argc;
 char** argv=s->argv;
 const Proto* f;
 int i;
 if (!lua_checkstack(L,argc)) fatal("too many input files");
 for (i=0; i<argc; i++)
 {
  const char* filename=IS("-") ? NULL : argv[i];
  if (luaL_loadfile(L,filename)!=0) fatal(lua_tostring(L,-1));
 }
 f=combine(L,argc);
 if (listing) luaU_print(f,listing>1);
 if (dumping)
 {
  FILE* D= (output==NULL) ? stdout : fopen(output,"wb");
  if (D==NULL) cannot("open");
  lua_lock(L);
  luaU_dumpEx(L,f,writer,D,stripping,&config);
  lua_unlock(L);
  if (ferror(D)) cannot("write");
  if (fclose(D)) cannot("close");
 }
 return 0;
}

int main(int argc, char* argv[])
{
 lua_State* L;
 struct Smain s;
 int i=doargs(argc,argv);
 argc-=i; argv+=i;
 if (argc<=0) usage("no input files given");
 L=lua_open();
 if (L==NULL) fatal("not enough memory for state");
 s.argc=argc;
 s.argv=argv;
 if (lua_cpcall(L,pmain,&s)!=0) fatal(lua_tostring(L,-1));
 lua_close(L);
 return EXIT_SUCCESS;
}
