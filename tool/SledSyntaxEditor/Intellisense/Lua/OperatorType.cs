/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// Specifies the type of an operator.
    /// </summary>
    public enum OperatorType
    {
        /// <summary>
        /// No valid operator type.
        /// </summary>
        None,

        // Binary operators

        /// <summary>
        /// An addition operator.
        /// </summary>
        Addition,

        /// <summary>
        /// A subtraction operator.
        /// </summary>
        Subtraction,

        /// <summary>
        /// A multiplication operator.
        /// </summary>
        Multiplication,

        /// <summary>
        /// A division operator.
        /// </summary>
        Division,

        /// <summary>
        /// A modulus operator.
        /// </summary>
        Modulus,

        /// <summary>
        /// A power operator.
        /// </summary>
        Power,

        /// <summary>
        /// An equality operator.
        /// </summary>
        Equality,

        /// <summary>
        /// An inequality operator.
        /// </summary>
        Inequality,

        /// <summary>
        /// An less-than or equal operator.
        /// </summary>
        LessThanEqual,

        /// <summary>
        /// An greater-than or equal operator.
        /// </summary>
        GreaterThanEqual,

        /// <summary>
        /// An less-than operator.
        /// </summary>
        LessThan,

        /// <summary>
        /// An greater-than operator.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// A and operator.
        /// </summary>
        And,

        /// <summary>
        /// A or operator.
        /// </summary>
        Or,

        /// <summary>
        /// A concatenate operator.
        /// </summary>
        Concatenate,

        // Unary operators

        /// <summary>
        /// A negate operator.
        /// </summary>
        Negate,

        /// <summary>
        /// A not operator.
        /// </summary>
        Not,

        /// <summary>
        /// A length operator.
        /// </summary>
        Length,
    }
}