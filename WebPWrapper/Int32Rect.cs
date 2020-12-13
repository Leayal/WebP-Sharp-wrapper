using System;
using System.Collections.Generic;
using System.Text;

namespace WebPWrapper
{
    public struct Int32Rect
    {
        private readonly static Int32Rect s_empty = new Int32Rect(0, 0, 0, 0);
        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods


        /// <summary>
        /// Compares two Int32Rect instances for exact equality.
        /// Note that double values can acquire error when operated upon, such that
        /// an exact comparison between two values which are logically equal may fail.
        /// Furthermore, using this equality operator, Double.NaN is not equal to itself.
        /// </summary>
        /// <returns>
        /// bool - true if the two Int32Rect instances are exactly equal, false otherwise
        /// </returns>
        /// <param name='int32Rect1'>The first Int32Rect to compare</param>
        /// <param name='int32Rect2'>The second Int32Rect to compare</param>
        public static bool operator ==(Int32Rect int32Rect1, Int32Rect int32Rect2)
        {
            return int32Rect1.X == int32Rect2.X &&
                   int32Rect1.Y == int32Rect2.Y &&
                   int32Rect1.Width == int32Rect2.Width &&
                   int32Rect1.Height == int32Rect2.Height;
        }

        /// <summary>
        /// Compares two Int32Rect instances for exact inequality.
        /// Note that double values can acquire error when operated upon, such that
        /// an exact comparison between two values which are logically equal may fail.
        /// Furthermore, using this equality operator, Double.NaN is not equal to itself.
        /// </summary>
        /// <returns>
        /// bool - true if the two Int32Rect instances are exactly unequal, false otherwise
        /// </returns>
        /// <param name='int32Rect1'>The first Int32Rect to compare</param>
        /// <param name='int32Rect2'>The second Int32Rect to compare</param>
        public static bool operator !=(Int32Rect int32Rect1, Int32Rect int32Rect2)
        {
            return !(int32Rect1 == int32Rect2);
        }
        /// <summary>
        /// Compares two Int32Rect instances for object equality.  In this equality
        /// Double.NaN is equal to itself, unlike in numeric equality.
        /// Note that double values can acquire error when operated upon, such that
        /// an exact comparison between two values which
        /// are logically equal may fail.
        /// </summary>
        /// <returns>
        /// bool - true if the two Int32Rect instances are exactly equal, false otherwise
        /// </returns>
        /// <param name='int32Rect1'>The first Int32Rect to compare</param>
        /// <param name='int32Rect2'>The second Int32Rect to compare</param>
        public static bool Equals(Int32Rect int32Rect1, Int32Rect int32Rect2)
        {
            if (int32Rect1.IsEmpty)
            {
                return int32Rect2.IsEmpty;
            }
            else
            {
                return int32Rect1.X.Equals(int32Rect2.X) &&
                       int32Rect1.Y.Equals(int32Rect2.Y) &&
                       int32Rect1.Width.Equals(int32Rect2.Width) &&
                       int32Rect1.Height.Equals(int32Rect2.Height);
            }
        }

        /// <summary>
        /// Equals - compares this Int32Rect with the passed in object.  In this equality
        /// Double.NaN is equal to itself, unlike in numeric equality.
        /// Note that double values can acquire error when operated upon, such that
        /// an exact comparison between two values which
        /// are logically equal may fail.
        /// </summary>
        /// <returns>
        /// bool - true if the object is an instance of Int32Rect and if it's equal to "this".
        /// </returns>
        /// <param name='o'>The object to compare to "this"</param>
        public override bool Equals(object o)
        {
            if ((null == o) || !(o is Int32Rect))
            {
                return false;
            }

            Int32Rect value = (Int32Rect)o;
            return Int32Rect.Equals(this, value);
        }

        /// <summary>
        /// Equals - compares this Int32Rect with the passed in object.  In this equality
        /// Double.NaN is equal to itself, unlike in numeric equality.
        /// Note that double values can acquire error when operated upon, such that
        /// an exact comparison between two values which
        /// are logically equal may fail.
        /// </summary>
        /// <returns>
        /// bool - true if "value" is equal to "this".
        /// </returns>
        /// <param name='value'>The Int32Rect to compare to "this"</param>
        public bool Equals(Int32Rect value)
        {
            return Int32Rect.Equals(this, value);
        }
        /// <summary>
        /// Returns the HashCode for this Int32Rect
        /// </summary>
        /// <returns>
        /// int - the HashCode for this Int32Rect
        /// </returns>
        public override int GetHashCode()
        {
            if (IsEmpty)
            {
                return 0;
            }
            else
            {
                // Perform field-by-field XOR of HashCodes
                return X.GetHashCode() ^
                       Y.GetHashCode() ^
                       Width.GetHashCode() ^
                       Height.GetHashCode();
            }
        }

        #endregion Public Methods

        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        /// <summary>
        /// Constructor which sets the initial values to the values of the parameters.
        /// </summary>
        public Int32Rect(Int32 x,
                    Int32 y,
                    Int32 width,
                    Int32 height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Empty - a static property which provides an Empty Int32Rectangle.
        /// </summary>
        public static Int32Rect Empty
        {
            get
            {
                return s_empty;
            }
        }

        /// <summary>
        /// Returns true if this Int32Rect is the Empty integer rectangle.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (_x == 0) && (_y == 0) && (_width == 0) && (_height == 0);
            }
        }

        /// <summary>
        /// Returns true if this Int32Rect has area.
        /// </summary>
        public bool HasArea
        {
            get
            {
                return _width > 0 && _height > 0;
            }
        }

        #region Public Properties

        /// <summary>
        ///     X - int.  Default value is 0.
        /// </summary>
        public int X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }

        }

        /// <summary>
        ///     Y - int.  Default value is 0.
        /// </summary>
        public int Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }

        }

        /// <summary>
        ///     Width - int.  Default value is 0.
        /// </summary>
        public int Width
        {
            get
            {
                return _width;
            }

            set
            {
                _width = value;
            }

        }

        /// <summary>
        ///     Height - int.  Default value is 0.
        /// </summary>
        public int Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
            }

        }

        #endregion Public Properties

        

        //------------------------------------------------------
        //
        //  Internal Fields
        //
        //------------------------------------------------------

        #region Internal Fields


        internal int _x;
        internal int _y;
        internal int _width;
        internal int _height;


        #endregion Internal Fields

    }
}
