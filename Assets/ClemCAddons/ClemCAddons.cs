using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ClemCAddons.Utilities;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ClemCAddons
{
    #region Extensions
    /// <summary>A collection of extensions for various uses and purposes.</summary>
    public static class Extensions
    {
        #region Conditions
        private static List<int> Gate = new List<int>();
        /// <summary>
        /// Only triggers the first time the value is true, until false.
        /// </summary>
        /// <param name="value">The value to check against.</param>
        /// <param name="id">The unique identifier for this gate.</param>
        public static bool OnceIfTrueGate(this bool value, int id)
        {
            if (Gate.Contains(id))
            {
                if (value)
                    return false;
                else
                    Gate.Remove(id);
            }
            if (value)
                Gate.Add(id);
            return value;
        }
        #endregion Conditions
        //good
        #region Type
        /// <summary>
        /// Looks for a type by name in every assemblies of the current domain and returns it.
        /// </summary>
        public static Type GetType(this string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
        /// <summary>
        /// Get type with a qualified name, using the type name and assembly.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="assembly">The assembly to be used.</param>
        public static Type GetTypeQualified(this string typeName, string assembly)
        {
            return Type.GetType(typeName + ", " + assembly);
        }
        #endregion Type
        //good
        #region Byte formatting
        /// <summary>
        /// Cast a set of serialized bytes to set type.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <param name="type">The type to be cast to.</param>
        public static dynamic ToType(this byte[] bytes, Type type)
        {
            var binformatter = new BinaryFormatter();
            var stream = new MemoryStream(bytes);
            return Convert.ChangeType(binformatter.Deserialize(stream), type);
        } // noice. Am actually impressed with it
        /// <summary>
        /// Deserialize a set of bytes
        /// </summary>
        public static object ToObject(this byte[] bytes)
        {
            var binformatter = new BinaryFormatter();
            var stream = new MemoryStream(bytes);
            return binformatter.Deserialize(stream);
        }
        /// <summary>
        /// Serializes an object into bytes
        /// </summary>
        public static byte[] ToBytes(this object value)
        {
            var binformatter = new BinaryFormatter();
            var stream = new MemoryStream();
            binformatter.Serialize(stream, value);
            return stream.ToArray();
        }
        /// <summary>
        /// Serializes a string into bytes
        /// </summary>
        public static byte[] ToRawBytes(this string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }
        /// <summary>
        /// Deserializes bytes into a string
        /// </summary>
        public static string ToRawString(this byte[] value)
        {
            return Encoding.ASCII.GetString(value);
        }
        /// <summary>
        /// Turns an object into a stream
        /// </summary>
        public static Stream ToSerializedStream(this object value)
        {
            var binformatter = new BinaryFormatter();
            var stream = new MemoryStream();
            binformatter.Serialize(stream, value);
            return stream;
        }
        #endregion Byte formatting
        //good
        #region ArrayAdditions
        #region Remove
        /// <summary>
        /// Removes an item at a set index in the array
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public static T[] RemoveAt<T>(this T[] source, int index) // thanks stack overflow
        {
            if (index < source.Length)
            {
                T[] dest = new T[source.Length - 1];
                if (index > 0)
                    Array.Copy(source, 0, dest, 0, index);
                if (index < source.Length - 1)
                    Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
                Array.Copy(dest, source, dest.Length);
                Array.Resize(ref source, dest.Length);
                return source;
            }
            Debug.LogError("index out of range");
            return source;
        }
        /// <summary>
        /// Removes an item at a set index in the array
        /// </summary>
        /// <param name="source">A reference to the array to modify.</param>
        /// <param name="index">The index of the item to remove.</param>
        public static T[] RemoveAt<T>(ref T[] source, int index) // thanks stack overflow
        {
            if (index < source.Length)
            {
                T[] dest = new T[source.Length - 1];
                if (index > 0)
                    Array.Copy(source, 0, dest, 0, index);
                if (index < source.Length - 1)
                    Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
                Array.Copy(dest, source, dest.Length);
                Array.Resize(ref source, dest.Length);
                return source;
            }
            Debug.LogError("index out of range");
            return source;
        }
        /// <summary>
        /// Removes all instances of an item in the array
        /// </summary>
        /// <param name="toRemove">The item to remove.</param>
        public static T[] RemoveAll<T>(this T[] source, T toRemove) // thanks stack overflow
        {
            if (source.Length == 0)
                return source;
            for (int i = source.Length - 1; i >= 0; i--)
            {
                if (source[i].Equals(toRemove))
                {
                    source = source.RemoveAt(i);
                }
            }
            return source;
        }
        #endregion Remove
        #region Add
        /// <summary>
        /// Add a value at the end of the array
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="value">The value to add to the array.</param>
        public static T[] Add<T>(this T[] source, T value)
        {
            T[] r = new T[source.Length];
            Array.Copy(source, r, source.Length);
            Array.Resize(ref r, source.Length + 1);
            r[source.Length] = value;
            return r;
        }
        /// <summary>
        /// Insert a value at an index in the array
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="value">The value to insert in the array.</param>
        /// <param name="index">The index to insert the value at.</param>
        public static T[] Add<T>(this T[] source, T value, int index)
        {
            List<T> r = source.ToList();
            r.Insert(index, value);
            return r.ToArray();
        }
        #endregion Add
        #region AddValue
#if (UNITY_STANDALONE_WIN)

        /// <summary>Add a value to every value in the array</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="value">The value to add.</param>
        public static T[] AddValue<T>(this T[] source, T value)
        {
            dynamic result = source;
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = result[i] + value;
            }
            return source;
        }
        /// <summary>Substract a value from every value in the array</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="value">The value to add.</param>
        public static T[] SubstractValue<T>(this T[] source, T value)
        {
            dynamic result = source;
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = result[i] - value;
            }
            return source;
        }
        /// <summary>Multiply every value in the array</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="value">The value to multiply.</param>
        public static T[] MultiplyValue<T>(this T[] source, T value)
        {
            dynamic result = source;
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = result[i] * value;
            }
            return source;
        }
        /// <summary>Divide every value in the array</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="value">The value to divide.</param>
        public static T[] DivideValue<T>(this T[] source, T value)
        {
            dynamic result = source;
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = result[i] / value;
            }
            return source;
        }
#endif

        #endregion AddValue

        #region Find
        /// <summary>Find the index of an item.</summary>
        /// <param name="source">The source array.</param>
        /// <param name="value">The item to look for.</param>
        /// <returns>-1 if cannot be found.</returns>
        public static int FindIndex<T>(this T[] source, T value)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(source[i], value))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>Find an item.</summary>
        /// <param name="source">The source array.</param>
        /// <param name="value">The item to look for.</param>
        /// <returns>Default if cannot be found.</returns>
        public static T Find<T>(this T[] source, T value)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].Equals(value))
                {
                    return source[i];
                }
            }
            return default;
        }
        /// <summary>Find an item.</summary>
        /// <param name="source">The source array.</param>
        /// <param name="value">The item to look for.</param>
        /// <param name="defaultReturn">The value to return per default.</param>
        public static T Find<T>(this T[] source, T value, T defaultReturn)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].Equals(value))
                {
                    return source[i];
                }
            }
            return defaultReturn;
        }
        #endregion Find
        #region ToArray
        /// <summary>Creates an array with the given item</summary>
        /// <param name="firstObject">The first object.</param>
        public static T[] MakeArray<T>(this T firstObject)
        {
            return new T[] { firstObject };
        }
        /// <summary>Creates an array with the given items</summary>
        /// <param name="firstObject">The first object.</param>
        /// <param name="objects">Additional items.</param>
        public static T[] MakeArray<T>(this T firstObject, params T[] objects)
        {
            return new T[] { firstObject };
        }
        #endregion ToArray
        #region SetAt
#if (UNITY_STANDALONE_WIN)

        /// <summary>Set the value at an index</summary>
        /// <param name="source">The source array.</param>
        /// <param name="value">New value.</param>
        /// <param name="index">Index to replace at.</param>
        public static T[] SetAt<T>(this T[] source, T value, int index)
        {
            dynamic result = source;
            result[index] = value;
            return result;
        }
        /// <summary>Sets a value only if the index doesn't exist</summary>
        /// <param name="source">The source array.</param>
        /// <param name="value">New value.</param>
        /// <param name="index">Index to replace at.</param>
        public static T[] CreateIfNotAt<T>(this T[] source, T value, int index)
        {
            if (source.Length <= index)
            {
                Array.Resize(ref source, index + 1);
                dynamic result = source;
                result[index] = value;
                return result;
            }
            return source;
        }
#endif

        /// <summary>Set the value at an index, extends the array if it doesn't exist</summary>
        /// <param name="source">The source array.</param>
        /// <param name="value">New value.</param>
        /// <param name="index">Index to replace at.</param>
        public static T[] SetOrCreateAt<T>(this T[] source, T value, int index)
        {
            if (source == null)
                source = new T[0];
            if (source.Length <= index)
            {
                Array.Resize(ref source, index + 1);
            }
            T[] result = source;
            result[index] = value;
            return result;
        }
        #endregion SetAt
        #region Total
        /// <summary>Adds every value together</summary>
        /// <param name="source">The source array.</param>
        public static float Total(this float[] source)
        {
            float r = 0;
            for (int i = 0; i < source.Length; i++)
                r += source[i];
            return r;
        }
        /// <summary>Adds every value together</summary>
        /// <param name="source">The source array.</param>
        public static int Total(this int[] source)
        {
            int r = 0;
            for (int i = 0; i < source.Length; i++)
                r += source[i];
            return r;
        }
        /// <summary>Adds every value together</summary>
        /// <param name="source">The source array.</param>
        public static double Total(this double[] source)
        {
            double r = 0;
            for (int i = 0; i < source.Length; i++)
                r += source[i];
            return r;
        }
#endregion Total
#endregion ArrayAdditions
        //good
#region Dictionary Additions
        /// <summary>Add or replace a key in the dictionary</summary>
        /// <param name="dictionary">The source dictionary.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">New value.</param>
        public static Dictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return dictionary;
            }
            dictionary.Add(key, value);
            return dictionary;
        }
#endregion Dictioanry Additions
        //good
        // good
        // good
#region Texture Additions
        /// <summary>Rewrites and updates part of a Texture2D.</summary>
        /// <param name="texture">The source texture.</param>
        /// <param name="pos">The top left corner to edit from.</param>
        /// <param name="size">The size of the zone to edit.</param>
        /// <param name="pixels">The pixels to edit with.</param>
        /// <param name="drawOver">Whether to draw over the previous layer, or blend.</param>
        /// <param name="apply">Apply the modifications to the texture before returning it.</param>
        public static Texture2D Update(this Texture2D texture, Vector2Int pos, Vector2Int size, Color32[] pixels, bool drawOver = true, bool apply = false)
        {
            if (!drawOver)
            {
                pixels = pixels.BlendWith(texture.GetPixels(pos.x, pos.y, size.x, size.y).Color32());
            }
            texture.SetPixels32(pos.x, pos.y, size.x, size.y, pixels);
            if (apply)
            {
                texture.Apply(false);
            }
            return texture;
        }
#endregion Texture Additions
        // good
#region Color Additions
        /// <summary>Sets all pixels in the array to match a set color.</summary>
        /// <param name="pixels">The pixels to edit.</param>
        /// <param name="pixels">The color to use.</param>
        public static Color[] SetColor<Color>(this Color[] pixels, Color color)
        {
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = color;
            return pixels;
        }
        /// <summary>Turns a pixel array into a Color32 color space pixel array.</summary>
        /// <param name="colors">The source pixels.</param>
        public static Color32[] Color32(this Color[] colors)
        {
            Color32[] r = new Color32[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                r[i] = colors[i];
            }
            return r;
        }
        /// <summary>Blends two pixel array together. Both arrays must be identically sized</summary>
        /// <param name="colors">The source array.</param>
        /// <param name="pixels">The array to blend in.</param>
        /// <param name="amount">Percentage of the source array to use.</param>
        public static Color32[] BlendWith(this Color32[] colors, Color32[] pixels, double amount = 0.5)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = colors[i].BlendWith(pixels[i], amount);
            }
            return pixels;
        }
        /// <summary>Blends two pixels together.</summary>
        /// <param name="color">The source color.</param>
        /// <param name="backColor">The color to blend in.</param>
        /// <param name="amount">Percentage of the source pixel to use.</param>
        public static Color32 BlendWith(this Color32 color, Color32 backColor, double amount = 0.5)
        {
            byte r = (byte)((color.r * amount) + (backColor.r * (1 - amount)));
            byte g = (byte)((color.g * amount) + (backColor.g * (1 - amount)));
            byte b = (byte)((color.b * amount) + (backColor.b * (1 - amount)));
            byte a = (byte)((color.a * amount) + (backColor.a * (1 - amount)));
            return new Color32(r, g, b, a);
        }
        /// <summary>Turns a color into a Vector4 (r, g, b, a).</summary>
        /// <param name="color">The source color.</param>
        public static Vector4 ToVector4(this Color32 color)
        {
            Color t = color;
            return new Vector4(t.r, t.g, t.b, t.a);
        }
        /// <summary>Turns a color into a Vector4 (r, g, b, a).</summary>
        /// <param name="color">The source color.</param>
        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }
        /// <summary>Turns a Vector4 into a Color.</summary>
        /// <param name="vector">The source Vector4.</param>
        public static Color ToColor(this Vector4 vector)
        {
            return new Color(vector.x, vector.y, vector.z, vector.w);
        }
        /// <summary>Turns a Vector4 into a Color32 color.</summary>
        /// <param name="vector">The source Vector4.</param>
        public static Color32 ToColor32(this Vector4 vector)
        {
            return new Color(vector.x, vector.y, vector.z, vector.w);
        }
        /// <summary>Turns a color into an array of float (r, g, b, a).</summary>
        /// <param name="color">The source color.</param>
        public static float[] ToArray(this Color color)
        {
            return new float[] { color.r, color.g, color.b, color.a };
        }
        /// <summary>Sets the r component of a color.</summary>
        /// <param name="color">The source color.</param>
        /// <param name="r">The r component to set.</param>
        public static Color SetR(this Color color, float r)
        {
            return new Color(r, color.g, color.b, color.a);
        }
        /// <summary>Sets the g component of a color.</summary>
        /// <param name="color">The source color.</param>
        /// <param name="g">The g component to set.</param>
        public static Color SetG(this Color color, float g)
        {
            return new Color(color.r, g, color.b, color.a);
        }
        /// <summary>Sets the b component of a color.</summary>
        /// <param name="color">The source color.</param>
        /// <param name="b">The b component to set.</param>
        public static Color SetB(this Color color, float b)
        {
            return new Color(color.r, color.g, b, color.a);
        }
        /// <summary>Sets the a component of a color.</summary>
        /// <param name="color">The source color.</param>
        /// <param name="a">The a component to set.</param>
        public static Color SetA(this Color color, float a)
        {
            return new Color(color.r, color.g, color.b, a);
        }
        /// <summary>Sets the r, g and b components of a color. Leaves a as-is.</summary>
        /// <param name="color">The source color.</param>
        /// <param name="r">The r component to set.</param>
        /// <param name="g">The g component to set.</param>
        /// <param name="b">The b component to set.</param>
        public static Color SetRGB(this Color color, float r, float g, float b)
        {
            return new Color(r, g, b, color.a);
        }
        /// <summary>Sets all components of a color.</summary>
        /// <param name="color">The source color.</param>
        /// <param name="r">The r component to set.</param>
        /// <param name="g">The g component to set.</param>
        /// <param name="b">The b component to set.</param>
        /// <param name="a">The a component to set.</param>
        public static Color Set(this Color _, float r, float g, float b, float a)
        {
            return new Color(r, g, b, a);
        }
#endregion Color Additions
        // good
#region Vector3 Additions
#region Geometry
        /// <summary>Gets the position of a specified corner or  of a rectangular geometry. Can also be used for other purposes</summary>
        /// <param name="pos">The position of the center of the rectangle.</param>
        /// <param name="corner">The normalized direction of the target corner.</param>
        /// <param name="scale">The scale of the rectangle.</param>
        public static Vector3 GetCorner(this Vector3 pos, Vector3 corner, Vector3 scale)
        {
            return pos + corner.Multiply(scale / 2);
        }
#endregion Geometry
#region Operations
        /// <summary>Adds a value to every component of a Vector3</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The value to add.</param>
        public static Vector3 Add(this Vector3 vector, float value)
        {
            return new Vector3(vector.x + value, vector.y + value, vector.z + value);
        }
        /// <summary>Substracts a value to every component of a Vector3</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The value to substract.</param>
        public static Vector3 Substract(this Vector3 vector, float value)
        {
            return new Vector3(vector.x - value, vector.y - value, vector.z - value);
        }
        /// <summary>Multiplies a value to every component of a Vector3</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The value to multiply with.</param>
        public static Vector3 Multiply(this Vector3 vector, float value)
        {
            return new Vector3(vector.x * value, vector.y * value, vector.z * value);
        }
        /// <summary>Divides a value to every component of a Vector3</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The value to divide by.</param>
        public static Vector3 Divide(this Vector3 vector, float value)
        {
            return new Vector3(vector.x / value, vector.y / value, vector.z / value);
        }
        /// <summary>Multiplies a Vector3 by another</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The vector to use.</param>
        public static Vector3 Multiply(this Vector3 vector, Vector3 value)
        {
            return new Vector3(vector.x * value.x, vector.y * value.y, vector.z * value.z);
        }
        /// <summary>Divides a Vector3 by another</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The vector to use.</param>
        public static Vector3 Divide(this Vector3 vector, Vector3 value)
        {
            return new Vector3(vector.x / value.x, vector.y / value.y, vector.z / value.z);
        }
        /// <summary>Inverses a Vector3's direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Inverse(this Vector3 vector)
        {
            return new Vector3(vector.x == 0 ? vector.x : 1 / vector.x, vector.y == 0 ? vector.y : 1 / vector.y, vector.z == 0 ? vector.z : 1 / vector.z);
        }
#endregion Operations
#region Validation
        /// <summary>Is the vector equal to (0, 0, 0)?</summary>
        /// <param name="vector">The source vector.</param>
        public static bool IsZero(this Vector3 vector)
        {
            return vector.Equals(Vector3.zero);
        }
        /// <summary>Is the vector equal to positive or negative infinity?</summary>
        /// <param name="vector">The source vector.</param>
        public static bool IsInfinite(this Vector3 vector)
        {
            return vector.Equals(Vector3.positiveInfinity) || vector.Equals(Vector3.negativeInfinity);
        }
        /// <summary>If the source value is zero, returns the chosen value</summary>
        /// <param name="f">The source value.</param>
        /// <param name="value">The value to replace 0 with.</param>
        public static float IfZero(this float f, float value)
        {
            return f.Equals(0) ? value : f;
        }
        /// <summary>If any parameter of the source value is zero, replace it by the matching parameter of the chosen value</summary>
        /// <param name="vector">The source value.</param>
        /// <param name="value">The value to replace 0 with.</param>
        public static Vector3 IfZero(this Vector3 vector, Vector3 value)
        {
            return vector.SetX(vector.x.IfZero(value.x)).SetY(vector.y.IfZero(value.y)).SetZ(vector.z.IfZero(value.z));
        }
        /// <summary>If any parameter of the source value is zero, replace it by the chosen value</summary>
        /// <param name="vector">The source value.</param>
        /// <param name="value">The value to replace 0 with.</param>
        public static Vector3 IfZero(this Vector3 vector, float value)
        {
            return vector.SetX(vector.x.IfZero(value)).SetY(vector.y.IfZero(value)).SetZ(vector.z.IfZero(value));
        }
        /// <summary>If the source value is not zero, returns the chosen value</summary>
        /// <param name="f">The source value.</param>
        /// <param name="value">The value to replace n0 with.</param>
        /// <param name="keepSign">Should the sign stay the same as the original value?</param>
        public static float IfNZero(this float f, float value, bool keepSign = false)
        {
            return f.Equals(0) ? f : value * (keepSign ? Mathf.Sign(f) : 1);
        }
        /// <summary>If any parameter of the source value is not zero, replace it by the matching parameter of the chosen value</summary>
        /// <param name="vector">The source value.</param>
        /// <param name="value">The value to replace n0 with.</param>
        /// <param name="keepSign">Should the sign stay the same as the original value?</param>
        public static Vector3 IfNZero(this Vector3 vector, Vector3 value, bool keepSign = false)
        {
            return vector.SetX(vector.x.IfNZero(value.x, keepSign)).SetY(vector.y.IfNZero(value.y, keepSign)).SetZ(vector.z.IfNZero(value.z, keepSign));
        }
        /// <summary>If any parameter of the source value is not zero, replace it by the chosen value</summary>
        /// <param name="vector">The source value.</param>
        /// <param name="value">The value to replace n0 with.</param>
        /// <param name="keepSign">Should the sign stay the same as the original value?</param>
        public static Vector3 IfNZero(this Vector3 vector, float value, bool keepSign = false)
        {
            return vector.SetX(vector.x.IfNZero(value, keepSign)).SetY(vector.y.IfNZero(value, keepSign)).SetZ(vector.z.IfNZero(value, keepSign));
        }
        /// <summary>Is approximately equal to another vector?</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The vector to compare with.</param>
        public static bool ApproximatelyEqual(this Vector3 vector, Vector3 value)
        {
            return vector.Distance(value) <= float.Epsilon;
        }
        /// <summary>Is approximately equal to another vector?</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The vector to compare with.</param>
        /// <param name="epsilon">The smallest difference for which they can be considered equal.</param>
        public static bool ApproximatelyEqual(this Vector3 vector, Vector3 value, float epsilon)
        {
            return vector.Distance(value) <= epsilon;
        }
#endregion Validation
#region Direction
        /// <summary>Turn the vector to match the direction</summary>
        /// <param name="vector">The source vector to turn.</param>
        /// <param name="direction">The direction vector.</param>
        public static Vector3 Remap(this Vector3 vector, Vector3 direction)
        {
            return (direction.Right() * vector.x)
                   + (direction.Up() * vector.y)
                   + (direction.Forward() * vector.z);
        }
        /// <summary>Rotate a vector on an axis</summary>
        /// <param name="vector">The source vector to turn.</param>
        /// <param name="axis">The axis to turn on.</param>
        /// <param name="angle">The angle to turn by.</param>
        public static Vector3 Rotate(this Vector3 vector, Vector3 axis, float angle) // https://answers.unity.com/questions/46770/rotate-a-vector3-direction.html
        {
            return Quaternion.AngleAxis(angle, axis) * vector;
        }
        /// <summary>Rotate a vector by its reference (in degrees)</summary>
        /// <param name="vector">The source vector to turn.</param>
        /// <param name="referenceVector">The degrees to turn it by in each axis.</param>
        public static Vector3 RotateAlong(this Vector3 vector, Vector3 referenceVector)
        {
            return Quaternion.Euler(referenceVector) * vector;
        }
        /// <summary>Using the source vector as forward, get the right direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Right(this Vector3 vector)
        {
            return -Vector3.Cross(vector.normalized, Vector3.up) * vector.magnitude;
        }
        /// <summary>Using the source vector as forward, get the left direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Left(this Vector3 vector)
        {
            return Vector3.Cross(vector.normalized, Vector3.up) * vector.magnitude;
        }
        /// <summary>Using the source vector as forward, get the up direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Up(this Vector3 vector)
        {
            return Vector3.Cross(Vector3.Cross(vector.normalized, Vector3.up), vector.normalized) * vector.magnitude;
        }
        /// <summary>Using the source vector as forward, get the up direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Down(this Vector3 vector)
        {
            return -Vector3.Cross(Vector3.Cross(vector.normalized, Vector3.up), vector.normalized) * vector.magnitude;
        }
        /// <summary>Returns the normalized source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Forward(this Vector3 vector)
        {
            return vector.normalized;
        }
        /// <summary>Using the source vector as forward, get the back direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Back(this Vector3 vector)
        {
            return -vector.normalized;
        }
        /// <summary>Get the direction between two vectors</summary>
        /// <param name="from">The origin vector.</param>
        /// <param name="to">The destination vector.</param>
        public static Vector3 Direction(this Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }
        /// <summary>Get the direction between two vectors</summary>
        /// <param name="from">The origin vector.</param>
        /// <param name="to">The destination vector.</param>
        /// <param name="inverse">Should origin and destination be inversed?</param>
        public static Vector3 Direction(this Vector3 from, Vector3 to, bool inverse)
        {
            return inverse ? (from - to).normalized : (to - from).normalized;
        }
        /// <summary>Returns the direction as a quaternion</summary>
        /// <param name="from">The origin vector.</param>
        /// <param name="to">The destination vector.</param>
        public static Quaternion FullDirection(this Vector3 from, Vector3 to)
        {
            Quaternion q;
            var a = Vector3.Dot(from, to);
            q.x = q.y = q.z = a;
            q.w = Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude) + Vector3.Dot(from, to);
            return q.normalized;
        }
        /// <summary>Turns a direction vector into a quaternion, using the Math module for operations</summary>
        /// <param name="vector">The source vector.</param>
        public static Quaternion FullToQuaternion(this Vector3 vector)
        {
            var angle = Math.Atan2(vector.z, vector.x); // Note: I expected atan2(z,x) but OP reported success with atan2(x,z) instead! Switch around if you see 90° off.
            return new Quaternion(0, (float)(1 * Math.Sin(angle / 2)), 0, (float)Math.Cos(angle / 2));
        }
        /// <summary>Turns a direction vector (angles) into a quaternion</summary>
        /// <param name="euler">The source vector.</param>
        public static Quaternion ToQuaternion(this Vector3 euler)
        {
            return Quaternion.Euler(euler);
        }
        /// <summary>Turns a direction vector into a quaternion, using Quaternion.LookRotation</summary>
        /// <param name="direction">The source vector.</param>
        public static Quaternion DirectionToQuaternion(this Vector3 direction)
        {
            return Quaternion.LookRotation(direction, Vector3.up);
        }
        /// <summary>Turns a local direction vector into a higher-space quaternion, using Quaternion.FromToRotation</summary>
        /// <param name="NormalizedVector">The source vector.</param>
        /// <param name="rotation">The context quaternion.</param>
        public static Quaternion ToQuaternion(this Vector3 NormalizedVector, Quaternion rotation)
        {
            return Quaternion.FromToRotation(Vector3.up, NormalizedVector.normalized) * rotation;
        }
        /// <summary>Reflects a direction vector perpendicular to a normal</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">The normal.</param>
        public static Vector3 Reflect(this Vector3 vector, Vector3 normal)
        {
            return vector - 2 * Vector3.Dot(vector, normal) * normal;
        }
        /// <summary>Turn a vector to move along a surface, given its normal</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">The normal.</param>
        public static Vector3 MoveAlong(this Vector3 vector, Vector3 normal)
        {
            return vector - Vector3.Dot(vector, normal) * normal;
        }
        /// <summary>Using the source vector as up, get the right direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector2 Right(this Vector2 vector)
        {
            return Vector2.Perpendicular(Vector2.Perpendicular(Vector2.Perpendicular(vector))).normalized;
        }
        /// <summary>Using the source vector as up, get the left direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector2 Left(this Vector2 vector)
        {
            return Vector2.Perpendicular(vector).normalized;
        }
        /// <summary>Returns the normalized vector</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector2 Up(this Vector2 vector)
        {
            return vector.normalized;
        }
        /// <summary>Using the source vector as up, get the down direction</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector2 Down(this Vector2 vector)
        {
            return Vector2.Perpendicular(Vector2.Perpendicular(vector)).normalized;
        }
#endregion Direction
#region Min Max
        /// <summary>For each parameter of the vector, returns the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector3 Min(this Vector3 vector, float min)
        {
            return new Vector3(vector.x.Min(min), vector.y.Min(min), vector.z.Min(min));
        }
        /// <summary>For each parameter of the vector, returns the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="ignoreSign">Should sign be ignored?</param>
        public static Vector3 Min(this Vector3 vector, float min, bool ignoreSign)
        {
            if (!ignoreSign)
                return vector.Min(min);
            return new Vector3(vector.x.Abs() > min.Abs() ? vector.x : min, vector.y.Abs() > min.Abs() ? vector.y : min, vector.z.Abs() > min.Abs() ? vector.z : min);
        }
        /// <summary>For each parameter of the vector, returns the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3 Max(this Vector3 vector, float max)
        {
            return new Vector3(vector.x.Max(max), vector.y.Max(max), vector.z.Max(max));
        }
        /// <summary>For each parameter of the vector, returns the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="ignoreSign">Should sign be ignored?</param>
        public static Vector3 Max(this Vector3 vector, float max, bool ignoreSign)
        {
            if (!ignoreSign)
                return vector.Max(max);
            return new Vector3(vector.x.Abs() > max.Abs() ? vector.x : max, vector.y.Abs() > max.Abs() ? vector.y : max, vector.z.Abs() > max.Abs() ? vector.z : max);
        }
        /// <summary>Returns the vector with its x parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3 MaxX(this Vector3 vector, float max)
        {
            return new Vector3(vector.x.Max(max), vector.y, vector.z);
        }
        /// <summary>Returns the vector with its y parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3 MaxY(this Vector3 vector, float max)
        {
            return new Vector3(vector.x, vector.y.Max(max), vector.z);
        }
        /// <summary>Returns the vector with its z parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3 MaxZ(this Vector3 vector, float max)
        {
            return new Vector3(vector.x, vector.y, vector.z.Max(max));
        }
        /// <summary>Returns the vector with its x and z parameter containing the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3 MaxXZ(this Vector3 vector, float max)
        {
            return new Vector3(vector.x.Max(max), vector.y, vector.z.Max(max));
        }
        /// <summary>Returns the vector with its x parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3 MinX(this Vector3 vector, float min)
        {
            return new Vector3(vector.x.Min(min), vector.y, vector.z);
        }
        /// <summary>Returns the vector with its y parameter containing the minimum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector3 MinY(this Vector3 vector, float min)
        {
            return new Vector3(vector.x, vector.y.Min(min), vector.z);
        }
        /// <summary>Returns the vector with its z parameter containing the minimum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector3 MinZ(this Vector3 vector, float min)
        {
            return new Vector3(vector.x, vector.y, vector.z.Min(min));
        }
        /// <summary>Returns the vector with its x and z parameter containing the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector3 MinXZ(this Vector3 vector, float min)
        {
            return new Vector3(vector.x.Min(min), vector.y, vector.z.Min(min));
        }
        /// <summary>Returns the smallest parameter in the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static int Min(this Vector3Int vector)
        {
            return vector.x.Min(vector.y);
        }
        /// <summary>Returns the biggest parameter in the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static int Max(this Vector3Int vector)
        {
            return vector.x.Max(vector.y);
        }
        /// <summary>For each parameter of the vector, returns the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector3Int Min(this Vector3Int vector, int min)
        {
            return new Vector3Int(vector.x.Min(min), vector.y.Min(min), vector.z.Min(min));
        }
        /// <summary>For each parameter of the vector, returns the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="ignoreSign">Should sign be ignored?</param>
        public static Vector3Int Min(this Vector3Int vector, int min, bool ignoreSign)
        {
            if (!ignoreSign)
                return vector.Min(min);
            return new Vector3Int(vector.x.Abs() > min.Abs() ? vector.x : min, vector.y.Abs() > min.Abs() ? vector.y : min, vector.z.Abs() > min.Abs() ? vector.z : min);
        }
        /// <summary>For each parameter of the vector, returns the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3Int Max(this Vector3Int vector, int max)
        {
            return new Vector3Int(vector.x.Max(max), vector.y.Max(max), vector.z.Max(max));
        }
        /// <summary>For each parameter of the vector, returns the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="ignoreSign">Should sign be ignored?</param>
        public static Vector3Int Max(this Vector3Int vector, int max, bool ignoreSign)
        {
            if (!ignoreSign)
                return vector.Max(max);
            return new Vector3Int(vector.x.Abs() > max.Abs() ? vector.x : max, vector.y.Abs() > max.Abs() ? vector.y : max, vector.z.Abs() > max.Abs() ? vector.z : max);
        }
        /// <summary>Returns the vector with its x parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3Int MaxX(this Vector3Int vector, int max)
        {
            return new Vector3Int(vector.x.Max(max), vector.y, vector.z);
        }
        /// <summary>Returns the vector with its y parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3Int MaxY(this Vector3Int vector, int max)
        {
            return new Vector3Int(vector.x, vector.y.Max(max), vector.z);
        }
        /// <summary>Returns the vector with its z parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3Int MaxZ(this Vector3Int vector, int max)
        {
            return new Vector3Int(vector.x, vector.y, vector.z.Max(max));
        }
        /// <summary>Returns the vector with its x and z parameter containing the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3Int MaxXZ(this Vector3Int vector, int max)
        {
            return new Vector3Int(vector.x.Max(max), vector.y, vector.z.Max(max));
        }
        /// <summary>Returns the vector with its x parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector3Int MinX(this Vector3Int vector, int min)
        {
            return new Vector3Int(vector.x.Min(min), vector.y, vector.z);
        }
        /// <summary>Returns the vector with its y parameter containing the minimum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector3Int MinY(this Vector3Int vector, int min)
        {
            return new Vector3Int(vector.x, vector.y.Min(min), vector.z);
        }
        /// <summary>Returns the vector with its z parameter containing the minimum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector3Int MinZ(this Vector3Int vector, int min)
        {
            return new Vector3Int(vector.x, vector.y, vector.z.Min(min));
        }
        /// <summary>Returns the vector with its x and z parameter containing the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector3Int MinXZ(this Vector3Int vector, int min)
        {
            return new Vector3Int(vector.x.Min(min), vector.y, vector.z.Min(min));
        }
#endregion Min Max
#region Self Min Max
        /// <summary>Returns the highest parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static float Max(this Vector3 vector)
        {
            return vector.x.Max(vector.y).Max(vector.z);
        }
        /// <summary>Returns the lowest parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static float Min(this Vector3 vector)
        {
            return vector.x.Min(vector.y).Min(vector.z);
        }
#endregion Self Min Max
#region Value
        /// <summary>Returns the absolute of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Abs(this Vector3 vector)
        {
            return new Vector3(vector.x.Abs(), vector.y.Abs(), vector.z.Abs());
        }
        /// <summary>Returns total of every parameter in the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static float Total(this Vector3 vector)
        {
            return vector.x + vector.y + vector.z;
        }
#endregion Value
#region Clamp
        /// <summary>Clamps each parameter of the vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static Vector3 Clamp(this Vector3 vector, float min, float max)
        {
            return new Vector3(vector.x.Clamp(min, max), vector.y.Clamp(min, max), vector.z.Clamp(min, max));
        }
        /// <summary>Clamps each parameter of the vector between 0 and 1</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Clamp01(this Vector3 vector)
        {
            return new Vector3(vector.x.Clamp01(), vector.y.Clamp01(), vector.z.Clamp01());
        }
        /// <summary>Clamps the x parameter of the vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static Vector3 ClampX(this Vector3 vector, float min, float max)
        {
            return new Vector3(vector.x.Clamp(min, max), vector.y, vector.z);
        }
        /// <summary>Clamps the y parameter of the vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static Vector3 ClampY(this Vector3 vector, float min, float max)
        {
            return new Vector3(vector.x, vector.y.Clamp(min, max), vector.z);
        }
        /// <summary>Clamps the z parameter of the vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static Vector3 ClampZ(this Vector3 vector, float min, float max)
        {
            return new Vector3(vector.x, vector.y, vector.z.Clamp(min, max));
        }
        /// <summary>Clamps the x and z parameters of the vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static Vector3 ClampXZ(this Vector3 vector, float min, float max)
        {
            return new Vector3(vector.x.Clamp(min, max), vector.y, vector.z.Clamp(min, max));
        }
        /// <summary>Clamps the total of the x and z parameters between (-max, max)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The max value.</param>
        public static Vector3 ClampXZTotal(this Vector3 vector, float max)
        {
            Vector2 r = new Vector2(vector.x.Abs(), vector.z.Abs()).normalized * max;
            return new Vector3(vector.x.Clamp(-r.x, r.x), vector.y, vector.z.Clamp(-r.y, r.y));
        }
        /// <summary>Clamps the x and z parameters while keeping the ratio between both</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static Vector3 ClampXZKeepRatio(this Vector3 vector, float min, float max)
        {
            if (vector.x != 0 && vector.z != 0)
            {
                if (vector.x > max || vector.z > max)
                {
                    if (vector.x > vector.z)
                    {
                        float coeff = vector.x / vector.z;
                        vector.x = vector.x.Clamp(min, max);
                        vector.z = vector.x / coeff;
                    }
                    else
                    {
                        float coeff = vector.z / vector.x;
                        vector.z = vector.z.Clamp(min, max);
                        vector.x = vector.z / coeff;
                    }
                }
                if (vector.x < min || vector.z < min)
                {
                    if (vector.x < vector.z)
                    {
                        float coeff = vector.x / vector.z;
                        vector.x = vector.x.Clamp(min, max);
                        vector.z = vector.x / coeff;
                    }
                    else
                    {
                        float coeff = vector.z / vector.x;
                        vector.z = vector.z.Clamp(min, max);
                        vector.x = vector.z / coeff;
                    }
                }
            }
            return new Vector3(vector.x.Clamp(min, max), vector.y, vector.z.Clamp(min, max));
        }
        /// <summary>Clamps the source vector between two vectors, at the individual parameter level</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min values.</param>
        /// <param name="max">The max values.</param>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return vector.SetX(vector.x.Clamp(min.x, max.x)).SetY(vector.y.Clamp(min.y, max.y)).SetZ(vector.z.Clamp(min.z, max.z));
        }
        /// <summary>Clamps each parameter of the source vector between (-max, max)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The max value.</param>
        public static Vector3 ClampAround0(this Vector3 vector, float max)
        {
            return vector.SetX(vector.x.Clamp(-max, max)).SetY(vector.y.Clamp(-max, max)).SetZ(vector.z.Clamp(-max, max));
        }
        /// <summary>Clamps each parameter of the source vector between each parameter of the max vector (-max, max)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The max values.</param>
        public static Vector3 ClampAround0(this Vector3 vector, Vector3 max)
        {
            return vector.SetX(vector.x.Clamp(-max.x, max.x)).SetY(vector.y.Clamp(-max.y, max.y)).SetZ(vector.z.Clamp(-max.z, max.z));
        }
        /// <summary>Clamps the parameters of the source vector to 0 only in the given direction</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="direction">The direction to clamp the vector in.</param>
        public static Vector3 ClampTo0InDirection(this Vector3 vector, Vector3 direction)
        {
            if (direction.x.Sign() > 0)
                vector.x = vector.x.Max(0, vector.x);
            else if (direction.x.Sign() < 0)
                vector.x = vector.x.Min(0, vector.x);
            if (direction.y.Sign() > 0)
                vector.y = vector.y.Max(0, vector.y);
            else if (direction.y.Sign() < 0)
                vector.y = vector.y.Min(0, vector.y);
            if (direction.z.Sign() > 0)
                vector.z = vector.z.Max(0, vector.z);
            else if (direction.z.Sign() < 0)
                vector.z = vector.z.Min(0, vector.z);
            return vector;
        }
        /// <summary>Clamps the parameters of the source vector to the target vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="target">The target vector the source vector is moving towards.</param>
        /// <param name="direction">The direction of approach.</param>
        public static Vector3 ClampTowards(this Vector3 vector, Vector3 target, Vector3 direction)
        {
            if (direction.x > 0 && vector.x > target.x)
                vector.x = target.x;
            if (direction.x < 0 && vector.x < target.x)
                vector.x = target.x;
            if (direction.y > 0 && vector.y > target.y)
                vector.y = target.y;
            if (direction.y < 0 && vector.y < target.y)
                vector.y = target.y;
            if (direction.z > 0 && vector.z > target.z)
                vector.z = target.z;
            if (direction.z < 0 && vector.z < target.z)
                vector.z = target.z;
            return vector;
        }
        /// <summary>Clamp the vector to a magnitude</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum magnitude.</param>
        public static Vector3 ClampMagnitude(this Vector3 vector, float max)
        {
            return vector.magnitude <= max ? vector : vector * max / vector.magnitude;
        }
        /// <summary>Clamp the vector between two magnitudes</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum magnitude.</param>
        /// <param name="max">The maximum magnitude.</param>
        public static Vector3 ClampMagnitude(this Vector3 vector, float min, float max)
        {
            if (vector.magnitude <= max && vector.magnitude >= min)
                return vector;
            if(vector.magnitude >= min)
                return vector * max / vector.magnitude;
            return vector * min / vector.magnitude;
        }
        /// <summary>Clamp the vector to a square magnitude (computes faster than magnitude)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum square magnitude.</param>
        public static Vector3 ClampSqrMagnitude(this Vector3 vector, float max)
        {
            return vector.sqrMagnitude <= max ? vector : vector * max / vector.sqrMagnitude;
        }
        /// <summary>Clamp the vector between two square magnitudes (computes faster than magnitude)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum square magnitude.</param>
        /// <param name="max">The maximum square magnitude.</param>
        public static Vector3 ClampSqrMagnitude(this Vector3 vector, float min, float max)
        {
            if (vector.sqrMagnitude <= max && vector.sqrMagnitude >= min)
                return vector;
            if (vector.sqrMagnitude >= min)
                return vector * max / vector.sqrMagnitude;
            return vector * min / vector.sqrMagnitude;
        }
        /// <summary>Clamp the vector's sum to a max value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum total.</param>
        public static Vector3 ClampTotal(this Vector3 vector, float max)
        {
            return vector.Total() <= max ? vector : vector * max / vector.Total();
        }
        /// <summary>Clamp the vector's sum to a max value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum total.</param>
        /// <param name="max">The maximum total.</param>
        public static Vector3 ClampTotal(this Vector3 vector, float min, float max)
        {
            if (vector.Total() <= max && vector.Total() >= min)
                return vector;
            if(vector.Total() >= min)
                return vector * max / vector.Total();
            return vector * min / vector.Total();
        }
        /// <summary>(Not validated) Clamp the vector's sum to a max value, but returns to max step by step (behaves like a Lerp)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="dt">The delta time to use for calculations. Is the time between each call of the function (can be multiplied by speed)</param>
        public static Vector3 ElasticClamp(this Vector3 vector, float max, float dt)
        {
            return vector.magnitude <= max ? vector : Vector3.Lerp(vector, vector * ((vector.magnitude - (max - vector.magnitude) / vector.magnitude)), dt);
        }
        /// <summary>Given a max vector, clamps the source vector circularly</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum vector (delimitating rectangle).</param>
        public static Vector3 CircularClamp(this Vector3 vector, Vector3 max)
        {
            if (vector.x == vector.y && vector.y == vector.z) // if 50%
                return vector.ClampTotal(max.Total()); // both are worth as much
            float total = vector.Abs().Total();
            float xRatio = vector.x.Abs() / total;
            float yRatio = vector.y.Abs() / total;
            float zRatio = vector.z.Abs() / total;
            float limit;
            limit = max.x * xRatio + max.y * yRatio + max.z * zRatio;
            return vector.ClampMagnitude(limit);
        }
#endregion Clamp
#region Add
        /// <summary>Returns the sum of every vector in the array</summary>
        /// <param name="vectors">The source vectors.</param>
        public static Vector3 Sum(this Vector3[] vectors)
        {
            Vector3 result = new Vector3 { };
            for (int i = 0; i < vectors.Length; i++)
            {
                result += vectors[i];
            }
            return result;
        }
#endregion Add
#region isBetween
        /// <summary>Is the source vector inside the bounding vector? (positional)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="bound">The bounding vector.</param>
        public static bool IsBetween(this Vector3 vector, Vector3 bound)
        {
            return vector.x.IsBetween(-bound.x, bound.x) && vector.y.IsBetween(-bound.y, bound.y) && vector.z.IsBetween(-bound.z, bound.z);
        }
        /// <summary>Is the source vector between both bounding vectors? (positional)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="bound1">The first bounding vector.</param>
        /// <param name="bound2">The second bounding vector.</param>
        public static bool IsBetween(this Vector3 vector, Vector3 bound1, Vector3 bound2)
        {
            return vector.x.IsBetween(bound1.x, bound2.x) && vector.y.IsBetween(bound1.y, bound2.y) && vector.z.IsBetween(bound1.z, bound2.z);
        }
#endregion isBetween
#region Set Partial
        /// <summary>Sets the x parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="x">The new x parameter.</param>
        public static Vector3 SetX(this Vector3 vector, float x)
        {
            vector.x = x;
            return vector;
        }
        /// <summary>Sets the y parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="y">The new y parameter.</param>
        public static Vector3 SetY(this Vector3 vector, float y)
        {
            vector.y = y;
            return vector;
        }
        /// <summary>Sets the z parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="z">The new z parameter.</param>
        public static Vector3 SetZ(this Vector3 vector, float z)
        {
            vector.z = z;
            return vector;
        }
        /// <summary>Sets the x parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="x">The new x parameter.</param>
        public static Vector3 SetX(this Vector3 vector, double x)
        {
            vector.x = (float)x;
            return vector;
        }
        /// <summary>Sets the y parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="y">The new y parameter.</param>
        public static Vector3 SetY(this Vector3 vector, double y)
        {
            vector.y = (float)y;
            return vector;
        }
        /// <summary>Sets the z parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="z">The new z parameter.</param>
        public static Vector3 SetZ(this Vector3 vector, double z)
        {
            vector.z = (float)z;
            return vector;
        }
#endregion Set Partial
#region ToVector2
        /// <summary>Returns the vector as a Vector2. The z parameter is dropped</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return vector;
        }
#endregion ToVector2
#region Random
        /// <summary>Returns a completely random vector between 0 and 1</summary>
        /// <param name="_">The source vector.</param>
        public static Vector3 Randomize(this Vector3 _)
        {
            return UnityEngine.Random.insideUnitSphere.normalized;
        }
        /// <summary>Replace selected parameters with a random value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="x">Should randomize x?</param>
        public static Vector3 Randomize(this Vector3 vector, bool x)
        {
            return new Vector3(x ? UnityEngine.Random.value : vector.x, UnityEngine.Random.value, UnityEngine.Random.value);
        }
        /// <summary>Replace selected parameters with a random value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="x">Should randomize x?</param>
        /// <param name="y">Should randomize y?</param>
        public static Vector3 Randomize(this Vector3 vector, bool x, bool y)
        {
            return new Vector3(x ? UnityEngine.Random.value : vector.x, y ? UnityEngine.Random.value : vector.y);
        }
        /// <summary>Replace selected parameters with a random value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="x">Should randomize x?</param>
        /// <param name="y">Should randomize y?</param>
        /// <param name="z">Should randomize z?</param>
        public static Vector3 Randomize(this Vector3 vector, bool x, bool y, bool z)
        {
            return new Vector3(x ? UnityEngine.Random.value : vector.x, y ? UnityEngine.Random.value : vector.y, z ? UnityEngine.Random.value : vector.z);
        }
        /// <summary>Randomize the source vector, only accepting a certain percentage of change from it.
        /// <br></br><br><i>To note this distribution is not equal. The lower the tolerance, the higher the chances of it being on the limits.</i></br> </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="tolerance">The accepted tolerance in percentage.</param>
        public static Vector3 Randomize(this Vector3 vector, float tolerance)
        {
            Vector3 r = UnityEngine.Random.insideUnitSphere.normalized;
            float diff = Vector3.Angle(vector, r) / 180; // perc diff
            float res = Mathf.Max(0, diff - tolerance); // perc diff that matters
            res = 1 - res; // perc to apply to new to get closer to old
            return Vector3.Lerp(r, vector, res).normalized; // a + (b -a ) * x
        }
        /// <summary>Randomize the source vector to a direction towards a random point in the bounds, only accepting a certain percentage of change from it
        /// <br></br><br><i>To note this distribution is not equal. The lower the tolerance, the higher the chances of it being on the limits</i></br> </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="tolerance">The accepted tolerance in percentage.</param>
        /// <param name="position">The position to calculate the direction from.</param>
        /// <param name="bounds">The bounds to get a random point from.</param>
        public static Vector3 RandomizeInBounds(this Vector3 vector, float tolerance, Vector3 position, Bounds bounds)
        {
            Vector3 r = position.Direction(GameTools.RandomPointInBounds(bounds));
            float diff = Vector3.Angle(vector, r) / 180; // perc diff
            float res = Mathf.Max(0, diff - tolerance); // perc diff that matters
            return Vector3.Slerp(r, vector, res).normalized; // a + (b -a ) * x
        }
#endregion Random
#region NormalizeTo
        /// <summary>Normalizes the source vector to a set length</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="n">The length to normalize to.</param>
        public static Vector3 NormalizeTo(this Vector3 vector, float n)
        {
            return vector.normalized * n;
        }
        /// <summary>Normalizes the source vector, then multiplies it by n
        /// <br></br><br></br>
        /// <i>This is close to averaging both directions</i></summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="n">The length to normalize to.</param>
        public static Vector3 NormalizeTo(this Vector3 vector, Vector3 n)
        {
            return vector.normalized.Multiply(n);
        }
#endregion NormalizeTo
#region Interpolation
        /// <summary>Interpolates between the source vector and b at a constant speed</summary>
        /// <param name="a">The source vector.</param>
        /// <param name="b">The target vector.</param>
        /// <param name="t">The speed to interpolate at.</param>
        public static Vector3 ConstantInterpolation(this Vector3 a, Vector3 b, float t)
        {
            return a + (b - a).normalized * t;
        }
        /// <summary>Interpolates between the source vector and b at a constant speed</summary>
        /// <param name="a">The source vector.</param>
        /// <param name="b">The target vector.</param>
        /// <param name="t">The speed to interpolate at.</param>
        public static Vector3 ClampedConstantInterpolation(this Vector3 a, Vector3 b, float t)
        {
            Vector3 r = a + (b - a).normalized * t;
            return r.ClampTowards(b, b - a);
        }
#endregion Interpolation
#region Sign
        /// <summary>Returns a vector representing the sign of each parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 Sign(this Vector3 vector)
        {
            return vector.SetX(vector.x.Sign()).SetY(vector.y.Sign()).SetZ(vector.z.Sign());
        }
        /// <summary>Returns the source vector with each parameters set to match the sign of the referenceVector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="referenceVector">The reference vector.</param>
        public static Vector3 SetSign(this Vector3 vector, Vector3 referenceVector)
        {
            return vector.Abs().Multiply(referenceVector.Sign());
        }
#endregion Sign
#endregion Vector3 Additions
        // good
#region Vector2 Additions
#region Validation
        /// <summary>Is approximately equal to another vector?</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The vector to compare with.</param>
        public static bool ApproximatelyEqual(this Vector2 vector, Vector2 value)
        {
            return vector.Distance(value) <= float.Epsilon;
        }
        /// <summary>Is approximately equal to another vector?</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="value">The vector to compare with.</param>
        /// <param name="epsilon">The smallest difference for which they can be considered equal.</param>
        public static bool ApproximatelyEqual(this Vector2 vector, Vector2 value, float epsilon)
        {
            return vector.Distance(value) <= epsilon;
        }
#endregion Validation
        // good
#region Clamp
        /// <summary>Clamps each parameter of the vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static Vector2 Clamp(this Vector2 vector, float min, float max)
        {
            return vector.SetX(vector.x.Clamp(min, max)).SetY(vector.y.Clamp(min, max));
        }
        /// <summary>Clamps the source vector between two vectors, at the individual parameter level</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The min values.</param>
        /// <param name="max">The max values.</param>
        public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max)
        {
            return vector.SetX(vector.x.Clamp(min.x, max.x)).SetY(vector.y.Clamp(min.y, max.y));
        }
        /// <summary>Clamps each parameter of the source vector between (-max, max)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The max value.</param>
        public static Vector2 ClampAround0(this Vector2 vector, float max)
        {
            return vector.SetX(vector.x.Clamp(-max, max)).SetY(vector.y.Clamp(-max, max));
        }
        /// <summary>Clamps each parameter of the source vector between each parameter of the max vector (-max, max)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The max values.</param>
        public static Vector2 ClampAround0(this Vector2 vector, Vector2 max)
        {
            return vector.SetX(vector.x.Clamp(-max.x, max.x)).SetY(vector.y.Clamp(-max.y, max.y));
        }
        /// <summary>Clamp the vector to a magnitude</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum magnitude.</param>
        public static Vector2 ClampMagnitude(this Vector2 vector, float max)
        {
            return vector.magnitude <= max ? vector : vector * max / vector.magnitude;
        }
        /// <summary>Clamp the vector to a square magnitude (computes faster than magnitude)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum square magnitude.</param>
        public static Vector2 ClampSqrMagnitude(this Vector2 vector, float max)
        {
            return vector.sqrMagnitude <= max ? vector : vector * max / vector.sqrMagnitude;
        }
        /// <summary>Clamp the vector's sum to a max value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum total.</param>
        public static Vector2 ClampTotal(this Vector2 vector, float max)
        {
            return vector.Total() <= max ? vector : vector * max / vector.Total();
        }
        /// <summary>Given a max vector, clamps the source vector circularly</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum vector (delimitating rectangle).</param>
        public static Vector2 CircularClamp(this Vector2 vector, Vector2 max)
        {
            if (vector.x.Abs() == vector.y.Abs()) // if 50%
                return vector.ClampTotal(max.Total()); // both are worth as much
            bool isX = vector.Abs().Min() == vector.x.Abs();
            var r = vector.Abs().Min() / vector.Abs().Max(); // % (ratio) between both
            float limit;
            if (isX)
            {
                limit = max.x * r + max.y * (1 - r);
            }
            else
            {
                limit = max.x * (1 - r) + max.y * r;
            }
            return vector.ClampMagnitude(limit);
        }
        /// <summary>Clamps the parameters of the source vector to 0 only in the given direction</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="direction">The direction to clamp the vector in.</param>
        public static Vector2 ClampTo0InDirection(this Vector2 vector, Vector2 direction)
        {
            if (direction.x.Sign() > 0)
                vector.x = vector.x.Max(0, vector.x);
            else if (direction.x.Sign() < 0)
                vector.x = vector.x.Min(0, vector.x);
            if (direction.y.Sign() > 0)
                vector.y = vector.y.Max(0, vector.y);
            else if (direction.y.Sign() < 0)
                vector.y = vector.y.Min(0, vector.y);
            return vector;
        }

        /// <summary>Clamps the parameters of the source vector to the target vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="target">The target vector the source vector is moving towards.</param>
        /// <param name="direction">The direction of approach.</param>
        public static Vector2 ClampTowards(this Vector2 vector, Vector2 target, Vector2 direction)
        {
            if (direction.x > 0 && vector.x > target.x)
                vector.x = target.x;
            if (direction.x < 0 && vector.x < target.x)
                vector.x = target.x;
            if (direction.y > 0 && vector.y > target.y)
                vector.y = target.y;
            if (direction.y < 0 && vector.y < target.y)
                vector.y = target.y;
            return vector;
        }
#endregion Clamp
        // good
#region Value
        /// <summary>Returns total of every parameter in the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static float Total(this Vector2 vector)
        {
            return vector.x + vector.y;
        }
        /// <summary>Returns the absolute of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector2 Abs(this Vector2 vector)
        {
            return new Vector2(vector.x.Abs(), vector.y.Abs());
        }
#endregion Value
        // good
#region Min Max
        /// <summary>For each parameter of the vector, returns the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector2 Min(this Vector2 vector, float min)
        {
            return new Vector2(vector.x.Min(min), vector.y.Min(min));
        }
        /// <summary>For each parameter of the vector, returns the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="ignoreSign">Should sign be ignored?</param>
        public static Vector2 Min(this Vector2 vector, float min, bool ignoreSign)
        {
            if (!ignoreSign)
                return vector.Min(min);
            return new Vector2(vector.x.Abs() > min.Abs() ? vector.x : min, vector.y.Abs() > min.Abs() ? vector.y : min);
        }
        /// <summary>For each parameter of the vector, returns the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector2 Max(this Vector2 vector, float max)
        {
            return new Vector2(vector.x.Max(max), vector.y.Max(max));
        }
        /// <summary>For each parameter of the vector, returns the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="ignoreSign">Should sign be ignored?</param>
        public static Vector2 Max(this Vector2 vector, float max, bool ignoreSign)
        {
            if (!ignoreSign)
                return vector.Max(max);
            return new Vector2(vector.x.Abs() > max.Abs() ? vector.x : max, vector.y.Abs() > max.Abs() ? vector.y : max);
        }
        /// <summary>Returns the vector with its x parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector2 MaxX(this Vector2 vector, float max)
        {
            return new Vector2(vector.x.Max(max), vector.y);
        }
        /// <summary>Returns the vector with its y parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector2 MaxY(this Vector2 vector, float max)
        {
            return new Vector2(vector.x, vector.y.Max(max));
        }
        /// <summary>Returns the vector with its x parameter containing the minimum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector2 MinX(this Vector2 vector, float min)
        {
            return new Vector2(vector.x.Min(min), vector.y);
        }
        /// <summary>Returns the vector with its y parameter containing the minimum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector2 MinY(this Vector2 vector, float min)
        {
            return new Vector2(vector.x, vector.y.Min(min));
        }
        /// <summary>Returns the smallest parameter in the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static int Min(this Vector2Int vector)
        {
            return vector.x.Min(vector.y);
        }
        /// <summary>Returns the biggest parameter in the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static int Max(this Vector2Int vector)
        {
            return vector.x.Max(vector.y);
        }
        /// <summary>For each parameter of the vector, returns the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector2Int Min(this Vector2Int vector, int min)
        {
            return new Vector2Int(vector.x.Min(min), vector.y.Min(min));
        }
        /// <summary>For each parameter of the vector, returns the minimum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="ignoreSign">Should sign be ignored?</param>
        public static Vector2Int Min(this Vector2Int vector, int min, bool ignoreSign)
        {
            if (!ignoreSign)
                return vector.Min(min);
            return new Vector2Int(vector.x.Abs() > min.Abs() ? vector.x : min, vector.y.Abs() > min.Abs() ? vector.y : min);
        }
        /// <summary>For each parameter of the vector, returns the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector2Int Max(this Vector2Int vector, int max)
        {
            return new Vector2Int(vector.x.Max(max), vector.y.Max(max));
        }
        /// <summary>For each parameter of the vector, returns the maximum between them and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="ignoreSign">Should sign be ignored?</param>
        public static Vector2Int Max(this Vector2Int vector, int max, bool ignoreSign)
        {
            if (!ignoreSign)
                return vector.Max(max);
            return new Vector2Int(vector.x.Abs() > max.Abs() ? vector.x : max, vector.y.Abs() > max.Abs() ? vector.y : max);
        }
        /// <summary>Returns the vector with its x parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector2Int MaxX(this Vector2Int vector, int max)
        {
            return new Vector2Int(vector.x.Max(max), vector.y);
        }
        /// <summary>Returns the vector with its y parameter containing the maximum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="max">The maximum.</param>
        public static Vector2Int MaxY(this Vector2Int vector, int max)
        {
            return new Vector2Int(vector.x, vector.y.Max(max));
        }
        /// <summary>Returns the vector with its x parameter containing the minimum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector2Int MinX(this Vector2Int vector, int min)
        {
            return new Vector2Int(vector.x.Min(min), vector.y);
        }
        /// <summary>Returns the vector with its y parameter containing the minimum between it and the value</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="min">The minimum.</param>
        public static Vector2Int MinY(this Vector2Int vector, int min)
        {
            return new Vector2Int(vector.x, vector.y.Min(min));
        }
#endregion Min Max
        // good
#region Self Min Max
        /// <summary>Returns the highest parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static float Max(this Vector2 vector)
        {
            return vector.x.Max(vector.y);
        }
        /// <summary>Returns the lowest parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        public static float Min(this Vector2 vector)
        {
            return vector.x.Min(vector.y);
        }
#endregion Self Min Max
        // good
#region Geometry
        /// <summary>Given a distance and angle, returns a point relative to the source point. Will always move in the positive direction</summary>
        /// <param name="point">The source point.</param>
        /// <param name="distance">The distance.</param>
        /// <param name="angle">The angle.</param>
        public static Vector2 PointOnSlope(this Vector2 point, float distance, float angle)
        {
            var r = angle.RadianToVector2() * distance;
            return point + new Vector2(r.x.Abs(), r.y.Abs());
        }
        /// <summary>Given a distance and angle, returns a point relative to the source point.</summary>
        /// <param name="point">The source point.</param>
        /// <param name="distanceHypotenuse">The distance.</param>
        /// <param name="angle">The angle.</param>
        public static Vector2 PointAtAngle(this Vector2 point, float distanceHypotenuse, float angle)
        {
            return point + (angle.DegreeToVector2() * distanceHypotenuse);
        }
        /// <summary>Turns degrees into a Vector2 direction</summary>
        /// <param name="degree">The source degrees.</param>
        public static Vector2 DegreeToVector2(this float degree)
        {
            return (degree * Mathf.Deg2Rad).RadianToVector2();
        }
        /// <summary>Turns radians into a Vector2 direction</summary>
        /// <param name="degree">The source radians.</param>
        public static Vector2 RadianToVector2(this float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }
#endregion Geometry
        // good
#region Direction
        /// <summary>Reflects a direction vector perpendicular to a normal</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">The normal.</param>
        public static Vector2 Reflect(this Vector2 vector, Vector2 normal)
        {
            return vector - 2 * Vector2.Dot(vector, normal) * normal;
        }
        /// <summary>Turn a vector to move along a surface, given its normal</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">The normal.</param>
        public static Vector2 Movealong(this Vector2 vector, Vector2 normal)
        {
            return vector - Vector2.Dot(vector, normal) * normal;
        }
        /// <summary>Get the direction between two vectors</summary>
        /// <param name="from">The origin vector.</param>
        public static Vector2 Direction(this Vector2 from, Vector2 to)
        {
            return (to - from).normalized;
        }
        /// <summary>Get the direction between two vectors</summary>
        /// <param name="from">The origin vector.</param>
        /// <param name="to">The destination vector.</param>
        /// <param name="inverse">Should origin and destination be inversed?</param>
        public static Vector2 Direction(this Vector2 from, Vector2 to, bool inverse)
        {
            return inverse ? (from - to).normalized : (to - from).normalized;
        }
#endregion Direction
        // good
#region isBetween
        /// <summary>Is the source vector inside the bounding vector? (positional)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="bound">The bounding vector.</param>
        public static bool IsBetween(this Vector2 vector, Vector2 bound)
        {
            return vector.x.IsBetween(-bound.x, bound.x) && vector.y.IsBetween(-bound.y, bound.y);
        }
        /// <summary>Is the source vector between both bounding vectors? (positional)</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="bound1">The first bounding vector.</param>
        /// <param name="bound2">The second bounding vector.</param>
        public static bool IsBetween(this Vector2 vector, Vector2 bound1, Vector2 bound2)
        {
            return vector.x.IsBetween(bound1.x, bound2.x) && vector.y.IsBetween(bound1.y, bound2.y);
        }
#endregion isBetween
        // good
#region Set Partial
        /// <summary>Sets the x parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="x">The new x parameter.</param>
        public static Vector2 SetX(this Vector2 vector, float x)
        {
            vector.x = x;
            return vector;
        }
        /// <summary>Sets the y parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="y">The new y parameter.</param>
        public static Vector2 SetY(this Vector2 vector, float y)
        {
            vector.y = y;
            return vector;
        }
        /// <summary>Sets the x parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="x">The new x parameter.</param>
        public static Vector2 SetX(this Vector2 vector, double x)
        {
            vector.x = (float)x;
            return vector;
        }
        /// <summary>Sets the y parameter of the source vector</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="y">The new y parameter.</param>
        public static Vector2 SetY(this Vector2 vector, double y)
        {
            vector.y = (float)y;
            return vector;
        }
#endregion Set Partial
        // good
#region ToVector3
        /// <summary>Returns the vector as a Vector3. The z parameter is 0</summary>
        /// <param name="vector">The source vector.</param>
        public static Vector3 ToVector3(this Vector2 vector)
        {
            return vector;
        }
        /// <summary>Returns the vector as a Vector3 with the set z parameter</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="z">The z parameter to use.</param>
        public static Vector3 ToVector3(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
#endregion ToVector3
        // good
#region Interpolation
        /// <summary>Interpolates from a to b at a constant speed, returns new a</summary>
        /// <param name="a">The source vector.</param>
        /// <param name="b">The destination vector.</param>
        /// <param name="t">The speed to move at.</param>
        public static Vector2 ConstantInterpolation(this Vector2 a, Vector2 b, float t)
        {
            return a + (a - b).normalized * t;
        }
        /// <summary>Interpolates between the source vector and b at a constant speed</summary>
        /// <param name="a">The source vector.</param>
        /// <param name="b">The target vector.</param>
        /// <param name="t">The speed to interpolate at.</param>
        public static Vector2 ClampedConstantInterpolation(this Vector2 a, Vector2 b, float t)
        {
            Vector2 r = a + (b - a).normalized * t;
            return r.ClampTowards(b, b - a);
        }
        // good
#endregion Interpolation
        // good
#region Operations
        public static Vector2 Add(this Vector2 vector, float value)
        {
            return new Vector2(vector.x + value, vector.y + value);
        }
        public static Vector2 Substract(this Vector2 vector, float value)
        {
            return new Vector2(vector.x - value, vector.y - value);
        }
        public static Vector2 Multiply(this Vector2 vector, float value)
        {
            return new Vector2(vector.x * value, vector.y * value);
        }
        public static Vector2 Divide(this Vector2 vector, float value)
        {
            return new Vector2(vector.x / value, vector.y / value);
        }
        public static Vector2 MultiplyEach(this Vector2 v, Vector2 vector)
        {
            return new Vector2(v.x * vector.x, v.y * vector.y);
        }

        public static Vector2 DivideEach(this Vector2 vector, Vector2 value)
        {
            return new Vector2(vector.x / value.x, vector.y / value.y);
        }
        public static Vector2 Multiply(this Vector2 vector, Vector2 value)
        {
            return new Vector2(vector.x * value.x, vector.y * value.y);
        }
        public static Vector2 Divide(this Vector2 vector, Vector2 value)
        {
            return new Vector2(vector.x / value.x, vector.y / value.y);
        }

        public static Vector2 Inverse(this Vector2 vector)
        {
            return new Vector2(vector.x == 0 ? vector.x : 1 / vector.x, vector.y == 0 ? vector.y : 1 / vector.y);
        }
#endregion Operations
#region Sign
        public static Vector2 Sign(this Vector2 vector)
        {
            return vector.SetX(vector.x.Sign()).SetY(vector.y.Sign());
        }
        public static Vector2 SetSign(this Vector2 vector, Vector2 referenceVector)
        {
            return vector.Abs().Multiply(referenceVector.Sign());
        }
#endregion Sign
#endregion Vector2 Additions
#region float Additions
#region Ratio
        public static float[] Ratio(this float value, params float[] values)
        {
            float total = value + values.Total();
            float[] r = new float[values.Length + 1];
            r[0] = total / value;
            for (int i = 0; i < values.Length; i++)
            {
                r[i + 1] = total / values[i];
            }
            return r;
        }
#endregion Ratio
#region RemoveInfinity
        public static float RemoveInfinity(this float value, bool InfiniteToMin = false)
        {
            if (value == float.PositiveInfinity)
                return InfiniteToMin ? float.MinValue : float.MaxValue;
            if (value == float.NegativeInfinity)
                return InfiniteToMin ? float.MaxValue : float.MinValue;
            return value;
        }
#endregion RemoveInfinity
#region GetMaxs
        public static float GetMax(this Vector3 vector, bool abs = false)
        {
            return vector.x.Max(abs, vector.y, vector.z);
        }

        public static float GetMaxXY(this Vector3 vector, bool abs = false)
        {
            return vector.x.Max(abs, vector.y);
        }
        public static float GetMaxXZ(this Vector3 vector, bool abs = false)
        {
            return vector.x.Max(abs, vector.z);
        }
        public static float GetMaxYZ(this Vector3 vector, bool abs = false)
        {
            return vector.y.Max(abs, vector.z);
        }
#endregion GetMaxs
#region Min Max
        public static float Min(this float value, params float[] values)
        {
            return Mathf.Min(value, values.Min());
        }
        public static float Max(this float value, params float[] values)
        {
            return Mathf.Max(value, values.Max());
        }
        public static float Min(this float value, bool abs = false, params float[] values)
        {
            return abs ? Mathf.Min(value.Abs(), values.Abs().Min()) : Mathf.Min(value, values.Min());
        }
        public static float Max(this float value, bool abs = false, params float[] values)
        {
            return abs ? Mathf.Max(value.Abs(), values.Abs().Max()) : Mathf.Max(value, values.Max());
        }
        public static float Min(this float value, bool abs = false, bool ignoreInfinity = false, params float[] values)
        {
            if (ignoreInfinity)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = values[i].RemoveInfinity(true);
                value = value.RemoveInfinity();
            }
            return abs ? Mathf.Min(value.Abs(), values.Abs().Min()) : Mathf.Min(value, values.Min());
        }
        public static float Max(this float value, bool abs = false, bool ignoreInfinity = false, params float[] values)
        {
            if (ignoreInfinity)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = values[i].RemoveInfinity(true);
                value = value.RemoveInfinity();
            }
            return abs ? Mathf.Max(value.Abs(), values.Abs().Max()) : Mathf.Max(value, values.Max());
        }
#endregion Min Max
#region Clamp
        public static float Clamp(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }
        public static double Clamp(this double value, double min, double max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
        public static float Clamp(this float value, double min, double max)
        {
            return Mathf.Clamp(value, (float)max, (float)min);
        }
        public static float Clamp01(this float value)
        {
            return Mathf.Clamp01(value);
        }
        public static float ClampAroundZero(this float value, float max)
        {
            return Mathf.Clamp(value, -max, max);
        }
        public static float ClampAroundZero(this float value, double max)
        {
            return Mathf.Clamp(value, (float)-max, (float)max);
        }
        public static float ClampTo0InDirection(this float value, float direction)
        {
            if (value.Sign() == direction.Sign()) // if the value is going in a direction matching it, it's going opposite to 0,
                return value;                     // nothing to worry about
            if (direction > 0) // & value is under 0
                value = value.Min(0);
            else // direction down & value is above 0
                value = value.Max(0);
            return value;
        }
        public static float BelowIs0(this float value, float check, float zero = 0)
        {
            return value < check ? zero : value;
        }
        public static float AboveIs0(this float value, float check, float zero = 0)
        {
            return value > check ? zero : value;
        }
#endregion Clamp
#region Abs, Sum, Minus
        public static float Abs(this float f)
        {
            return Mathf.Abs(f);
        }
        public static float[] Abs(this float[] f)
        {
            for (int i = 0; i < f.Length; i++)
                f[i] = f[i].Abs();
            return f;
        }
        public static float Sum(this Vector2 vector)
        {
            return vector.x + vector.y;
        }
        public static float Sum(this Vector3 vector)
        {
            return vector.x + vector.y + vector.z;
        }
        public static float Minus(this float f, float value, bool abs = false)
        {
            return abs ? Mathf.Abs(f - value) : f - value;
        }
        public static float Minus(this float f, float value, bool maxFirst = false, bool abs = false)
        {
            return abs ? (Mathf.Max(f, value) - Mathf.Min(f, value)).Abs() : Mathf.Max(f, value) - Mathf.Min(f, value);
        }
        public static float MinusAngle(this float f, float value)
        {
            float r = f - value;
            return r + ((r > 180) ? -360 : (r < -180) ? 360 : 0);
        }
        public static float MinusAngle(this float f, float value, bool maxFirst = false)
        {
            float r = Mathf.Max(f, value) - Mathf.Min(f, value);
            return r + ((r > 180) ? -360 : (r < -180) ? 360 : 0);
        }
#endregion Abs, Sum
#region IsBetween
        public static bool IsBetween(this float value, float first, float second)
        {
            return value >= Mathf.Min(first, second) && value <= Mathf.Max(first, second);
        }
#endregion IsBetween
#region Round
        public static int Round(this float _value)
        {
            return (int)Math.Round(_value);
        }
        public static int Truncate(this float _value)
        {
            return (int)_value;
        }
#endregion Round
#region Angles
        // saved me a lot of time man https://stackoverflow.com/a/42248572
        public static float ClampAngle(this float value, float min, float max)
        {
            value = NormalizeAngle(value);
            if (value.IsBetween(min, max))
                return value;
            if (value > max)
                return max;
            return min;
        }
        public static float NormalizeAngle(this float value, float to = 180)
        {
            return (value + 180) % (to * 2) - 180;
        }
#endregion Angles
#region Sign
        public static float Sign(this float value)
        {
            return Mathf.Sign(value);
        }
        public static float SignInt(this float value)
        {
            return Mathf.Sign(value).Round();
        }
        public static bool SignBool(this float value)
        {
            return Mathf.Sign(value) == 1;
        }
#endregion Sign
#endregion float Additions
#region int Additions
        public static bool IsBetween(this int value, int first, int second)
        {
            return value >= Math.Min(first, second) && value <= Math.Max(first, second);
        }
        public static bool IsBetween(this int value, float first, float second)
        {
            return value >= Math.Min(first, second) && value <= Math.Max(first, second);
        }
        public static bool IsBetween(this int value, Vector2Int limit)
        {
            return value >= limit.Min() && value <= limit.Max();
        }
#region Loop Around
        public static int LoopAround(this int value, int length)
        {
            if (value < length - 1)
                return value + 1;
            return 0;
        }
        public static int LoopAround(this int value, int length, int change)
        {
            value += change;
            if (value < length)
                return value;
            if (value > 0)
                return 0;
            if (value < 0)
                return length + value;
            return value - length;
        }
#endregion Loop Around
#region Min Max
        public static int Min(this int value, params int[] values)
        {
            return Mathf.Min(value, values.Min());
        }
        public static int Max(this int value, params int[] values)
        {
            return Mathf.Max(value, values.Max());
        }
        public static int Min(this int value, bool abs = false, params int[] values)
        {
            return abs ? Mathf.Min(value.Abs(), values.Abs().Min()) : Mathf.Min(value, values.Min());
        }
        public static int Max(this int value, bool abs = false, params int[] values)
        {
            return abs ? Mathf.Max(value.Abs(), values.Abs().Max()) : Mathf.Max(value, values.Max());
        }
#endregion Min Max

#region Abs, Sum, Minus
        public static int Abs(this int f)
        {
            return Mathf.Abs(f);
        }
        public static int[] Abs(this int[] f)
        {
            for (int i = 0; i < f.Length; i++)
                f[i] = f[i].Abs();
            return f;
        }
        public static int Sum(this Vector2Int vector)
        {
            return vector.x + vector.y;
        }
        public static int Sum(this Vector3Int vector)
        {
            return vector.x + vector.y + vector.z;
        }
        public static int Minus(this int f, int value, bool abs = false)
        {
            return abs ? Mathf.Abs(f - value) : f - value;
        }
        public static int Minus(this int f, int value, bool maxFirst = false, bool abs = false)
        {
            return abs ? (Mathf.Max(f, value) - Mathf.Min(f, value)).Abs() : Mathf.Max(f, value) - Mathf.Min(f, value);
        }
        public static int MinusAngle(this int f, int value)
        {
            int r = f - value;
            return r + ((r > 180) ? -360 : (r < -180) ? 360 : 0);
        }
        public static int MinusAngle(this int f, int value, bool maxFirst = false)
        {
            int r = Mathf.Max(f, value) - Mathf.Min(f, value);
            return r + ((r > 180) ? -360 : (r < -180) ? 360 : 0);
        }
#endregion Abs, Sum
#endregion int Additions
#region Basic conversions
        public static bool ToBool(this int value)
        {
            return value <= 0;
        }
        public static unsafe int ToInt(this bool value)
        {
            return *(Byte*)&value;
            // optimized pointer cast. Cost is basically 0 on intel cpu, 1 cycle on amd, according to my source (https://stackoverflow.com/questions/66985162/how-to-convert-bool-to-int-efficiently)
        }
#endregion Basic conversions
#region Transform Additions
#region Anchors
        public static Vector3 GetWorld(this Transform transform, Vector3 localSpace)
        {
            return transform.TransformVector(localSpace);
        }
        public static Vector3 GetWorldPosition(this Transform transform, Vector3 localSpace)
        {
            return transform.position + transform.TransformVector(localSpace);
        }
        public static Vector3 GetLocal(this Transform transform, Vector3 worldSpace)
        {
            return transform.InverseTransformVector(worldSpace);
        }
        public static Vector3 GetLocalPosition(this Transform transform, Vector3 worldSpace)
        {
            return transform.InverseTransformPoint(worldSpace);
        }
#endregion Anchors
        public static int ActiveChildCount(this Transform transform)
        {
            int r = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    r++;
                }
            }
            return r;
        }
        public static int ChildsOfTypeCount(this Transform transform, Type type)
        {
            int r = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetType() == type)
                {
                    r++;
                }
            }
            return r;
        }
        public static int ChildsWithComponent(this Transform transform, Type type)
        {
            int r = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(type, out _))
                {
                    r++;
                }
            }
            return r;
        }
        public static int ActiveChildsOfTypeCount(this Transform transform, Type type)
        {
            int r = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetType() == type && transform.GetChild(i).gameObject.activeSelf)
                {
                    r++;
                }
            }
            return r;
        }
        public static int ActiveChildsWithComponent(this Transform transform, Type type)
        {
            int r = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(type, out _) && transform.GetChild(i).gameObject.activeSelf)
                {
                    r++;
                }
            }
            return r;
        }
        public static Transform FindDeep(this Transform transform, string name, bool startsWith = false)
        {
            if (transform.childCount == 0)
                return null;
            Transform[] res = transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform r in res)
            {
                if (r.name == name || (startsWith && r.name.StartsWith(name)))
                {
                    return r;
                }
            }
            return null;
        }
        public static Transform FindDeep(this Transform transform, Predicate<Transform> predicate)
        {
            if (transform.childCount == 0)
                return null;
            Transform[] res = transform.GetComponentsInChildren<Transform>(true);
            var r = Array.Find(res, predicate);
            if (r != null)
                return r;
            return null;
        }
        public static GameObject FindDeep(this GameObject gameObject, string name, bool startsWith = false)
        {
            var r = gameObject.transform.FindDeep(name, startsWith);
            return r == null ? null : r.gameObject;
        }

        public static GameObject FindDeepUID(this GameObject gameObject, int uid)
        {
            Transform[] res = gameObject.GetComponentsInChildren<Transform>(true);
            foreach(Transform r in res)
            {
                if (r.GetInstanceID() == uid)
                {
                    return r.gameObject;
                }
                if (r.gameObject.GetInstanceID() == uid)
                {
                    return r.gameObject;
                }
                var components = r.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component.GetInstanceID() == uid)
                    {
                        return r.gameObject;
                    }
                }
            }
            return null;
        }


        public static Transform GetParent(this Transform transform, int num)
        {
            if (num == 0)
                return transform;
            return transform.parent.GetParent(num - 1);
        }
        public static Transform FindParentDeep(this Transform transform, string name)
        {
            if (transform == null)
                return null;
            Transform res = transform.parent;
            if (res == null)
                return null;
            if (res.name == name)
            {
                return res;
            }
            return FindParentDeep(res, name);
        }
        public static GameObject FindParentDeep(this GameObject gameObject, string name)
        {
            var r = gameObject.transform.FindParentDeep(name);
            return r == null ? null : r.gameObject;
        }
        public static Transform[] GetChildrenWithComponent(this Transform transform, Type type)
        {
            Transform[] r = new Transform[] { };
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(type, out _))
                {
                    r.Add(transform.GetChild(i));
                }
            }
            return r;
        }
        public static Transform FindParentWithComponent(this Transform transform, Type type)
        {
            if (transform == null)
                return null;
            Transform res = transform.parent;
            if (res == null)
                return null;
            if (res.TryGetComponent(type, out _))
            {
                return res;
            }
            return FindParentWithComponent(res, type);
        }
        public static GameObject FindParentWithComponent(this GameObject gameObject, Type type)
        {
            var r = gameObject.transform.FindParentWithComponent(type);
            return r == null ? null : r.gameObject;
        }
        public static Rect GetWorldRect(this RectTransform rt, Vector2? scale = null)
        {
            if (!scale.HasValue)
                scale = Vector2.one;
            // Convert the rectangle to world corners and grab the top left
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector3 topLeft = corners[0];

            // Rescale the size appropriately based on the current Canvas scale
            Vector2 scaledSize = new Vector2(scale.Value.x * rt.rect.size.x, scale.Value.y * rt.rect.size.y);

            return new Rect(topLeft, scaledSize);
        }
        public static Vector3 AnchorNeutralPosition(this RectTransform rectTransform)
        {
            return rectTransform.position + (rectTransform.rect.center * rectTransform.lossyScale).ToVector3();
        }
        public static Vector3 AnchorNeutralPosition(this RectTransform rectTransform, bool offset)
        {
            return rectTransform.position + ((rectTransform.rect.center - (offset ? rectTransform.anchoredPosition : Vector2.zero)) * rectTransform.lossyScale).ToVector3();
        }
#endregion Transform Additions
#region Distances
        public static float Distance(this Transform transform, Transform compared) // transform transform
        {
            return Vector3.Distance(transform.position, compared.position);
        }
        public static float Distance(this Transform transform, GameObject compared) // transform gameobject
        {
            return Vector3.Distance(transform.position, compared.transform.position);
        }
        public static float Distance(this Transform transform, Vector3 compared) // transform vector3
        {
            return Vector3.Distance(transform.position, compared);
        }
        public static float Distance(this GameObject transform, Transform compared) // gameobject transform
        {
            return Vector3.Distance(transform.transform.position, compared.position);
        }
        public static float Distance(this GameObject transform, GameObject compared) // gameobject gameobject
        {
            return Vector3.Distance(transform.transform.position, compared.transform.position);
        }
        public static float Distance(this GameObject transform, Vector3 compared) // gameobject vector3
        {
            return Vector3.Distance(transform.transform.position, compared);
        }
        public static float Distance(this Vector3 transform, Transform compared) // vector3 transform
        {
            return Vector3.Distance(transform, compared.position);
        }
        public static float Distance(this Vector3 transform, GameObject compared) // vector3 gameobject
        {
            return Vector3.Distance(transform, compared.transform.position);
        }
        public static float Distance(this Vector3 transform, Vector3 compared) // vector3 vector3
        {
            return Vector3.Distance(transform, compared);
        }
        public static float Distance(this Vector2 vector, Vector2 compared) // vector3 vector3
        {
            return Vector2.Distance(vector, compared);
        }
        public static Transform GetClosest(this Transform[] transforms, Transform referenceFrame)
        {
            Transform closest = transforms[0];
            float t = referenceFrame.Distance(transforms[0]);
            for (int i = 1; i < transforms.Length; i++)
            {
                if (referenceFrame.Distance(transforms[i]) < t)
                {
                    t = referenceFrame.Distance(transforms[i]);
                    closest = transforms[i];
                }
            }
            return closest;
        }
        public static Transform GetClosest(this Transform[] transforms, Vector3 referenceFrame)
        {
            Transform closest = transforms[0];
            float t = referenceFrame.Distance(transforms[0]);
            for (int i = 1; i < transforms.Length; i++)
            {
                if (referenceFrame.Distance(transforms[i]) < t)
                {
                    t = referenceFrame.Distance(transforms[i]);
                    closest = transforms[i];
                }
            }
            return closest;
        }
        public static Transform GetClosest(this Transform[] transforms, GameObject referenceFrame)
        {
            Transform closest = transforms[0];
            float t = referenceFrame.Distance(transforms[0]);
            for (int i = 1; i < transforms.Length; i++)
            {
                if (referenceFrame.Distance(transforms[i]) < t)
                {
                    t = referenceFrame.Distance(transforms[i]);
                    closest = transforms[i];
                }
            }
            return closest;
        }
        public static GameObject GetClosest(this GameObject[] gameObjects, Transform referenceFrame)
        {
            GameObject closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Length; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static GameObject GetClosest(this GameObject[] gameObjects, Vector3 referenceFrame)
        {
            GameObject closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Length; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static GameObject GetClosest(this GameObject[] gameObjects, GameObject referenceFrame)
        {
            GameObject closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Length; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static Vector3 GetClosest(this Vector3[] gameObjects, Transform referenceFrame)
        {
            Vector3 closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Length; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static Vector3 GetClosest(this Vector3[] gameObjects, Vector3 referenceFrame)
        {
            Vector3 closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Length; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static Vector3 GetClosest(this Vector3[] gameObjects, GameObject referenceFrame)
        {
            Vector3 closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Length; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }

        public static Transform GetClosest(this List<Transform> transforms, Transform referenceFrame)
        {
            Transform closest = transforms[0];
            float t = referenceFrame.Distance(transforms[0]);
            for (int i = 1; i < transforms.Count; i++)
            {
                if (referenceFrame.Distance(transforms[i]) < t)
                {
                    t = referenceFrame.Distance(transforms[i]);
                    closest = transforms[i];
                }
            }
            return closest;
        }
        public static Transform GetClosest(this List<Transform> transforms, Vector3 referenceFrame)
        {
            Transform closest = transforms[0];
            float t = referenceFrame.Distance(transforms[0]);
            for (int i = 1; i < transforms.Count; i++)
            {
                if (referenceFrame.Distance(transforms[i]) < t)
                {
                    t = referenceFrame.Distance(transforms[i]);
                    closest = transforms[i];
                }
            }
            return closest;
        }
        public static Transform GetClosest(this List<Transform> transforms, GameObject referenceFrame)
        {
            Transform closest = transforms[0];
            float t = referenceFrame.Distance(transforms[0]);
            for (int i = 1; i < transforms.Count; i++)
            {
                if (referenceFrame.Distance(transforms[i]) < t)
                {
                    t = referenceFrame.Distance(transforms[i]);
                    closest = transforms[i];
                }
            }
            return closest;
        }
        public static GameObject GetClosest(this List<GameObject> gameObjects, Transform referenceFrame)
        {
            GameObject closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Count; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static GameObject GetClosest(this List<GameObject> gameObjects, Vector3 referenceFrame)
        {
            GameObject closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Count; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static GameObject GetClosest(this List<GameObject> gameObjects, GameObject referenceFrame)
        {
            GameObject closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Count; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static Vector3 GetClosest(this List<Vector3> gameObjects, Transform referenceFrame)
        {
            Vector3 closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Count; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static Vector3 GetClosest(this List<Vector3> gameObjects, Vector3 referenceFrame)
        {
            Vector3 closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Count; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }
        public static Vector3 GetClosest(this List<Vector3> gameObjects, GameObject referenceFrame)
        {
            Vector3 closest = gameObjects[0];
            float t = referenceFrame.Distance(gameObjects[0]);
            for (int i = 1; i < gameObjects.Count; i++)
            {
                if (referenceFrame.Distance(gameObjects[i]) < t)
                {
                    t = referenceFrame.Distance(gameObjects[i]);
                    closest = gameObjects[i];
                }
            }
            return closest;
        }

#endregion Distances
#region Quaternion Additions
        public static bool IsApproximatelyEqual(this Quaternion quat, Quaternion quaternion, float range = 0.01f)
        {
            return 1 - Mathf.Abs(Quaternion.Dot(quat, quaternion)) < range;
        }
        public static Quaternion SetX(this Quaternion quaternion, float x)
        {
            quaternion.x = x;
            return quaternion;
        }
        public static Quaternion SetY(this Quaternion quaternion, float y)
        {
            quaternion.y = y;
            return quaternion;
        }
        public static Quaternion SetZ(this Quaternion quaternion, float z)
        {
            quaternion.z = z;
            return quaternion;
        }
        public static Quaternion SetW(this Quaternion quaternion, float w)
        {
            quaternion.w = w;
            return quaternion;
        }
        public static Quaternion Multiply(this Quaternion quaternion, float value)
        {
            return new Quaternion(quaternion.x * value, quaternion.y * value, quaternion.z * value, quaternion.w * value);
        }
        public static Quaternion Inverse(this Quaternion quaternion)
        {
            return Quaternion.Inverse(quaternion);
        }
#region Interpolation
        public static Quaternion Slerp(this Quaternion a, Quaternion b, float t)
        {
            return Quaternion.Slerp(a, b, t);
        }
        public static Quaternion ConstantInterpolation(this Quaternion a, Quaternion b, float t)
        {
            return Quaternion.RotateTowards(a, b, t);
        }
#endregion Interpolation
#endregion QuaternionAdditions
#region Class Additions
#if (UNITY_STANDALONE_WIN)

        public static T[] GetCopy<T>(this T[] source)
        {
            var t = new T[source.Length];
            source.CopyTo(t, 0);
            return t;
        }
        public static T Clone<T>(this T source) // inspired by https://stackoverflow.com/a/78612
        {
            return source.ToBytes().ToType(typeof(T));
        }
#endif
#endregion Class Additions
    }
#endregion Extensions
    namespace Utilities
    {
#region Utilities
        public class GameTools
        {
#region Logic
            private static List<KeyValuePair<int, bool>> GateMemory = new List<KeyValuePair<int, bool>>();
            public static bool OnceIfTrue(int id, bool value)
            {
                var c = GateMemory.FindIndex(t => t.Key == id);
                if (c != -1)
                {
                    bool t = value;
                    if (value)
                        GateMemory[c] = new KeyValuePair<int, bool>(id, false);
                    if (!value)
                        GateMemory[c] = new KeyValuePair<int, bool>(id, true);
                    return GateMemory[c].Value && t;
                }
                else
                {
                    GateMemory.Add(new KeyValuePair<int, bool>(id, !value));
                    return value;
                }
            }
#endregion Logic
#region Vector3 Additions
            public static Vector3 Rotate(Vector3 vector, float angle, Vector3 axis) // https://answers.unity.com/questions/46770/rotate-a-vector3-direction.html
            {
                return Quaternion.AngleAxis(angle, axis) * vector;
            }
            public static Vector3 Sum(Vector3[] vectors)
            {
                Vector3 result = new Vector3 { };
                for (int i = 0; i < vectors.Length; i++)
                {
                    result += vectors[i];
                }
                return result;
            }
#region Direction
            public static Vector3 DirectionTo(Vector3 from, Vector3 to)
            {
                return (to - from).normalized;
            }
            public static Vector3 Direction(Vector3 from, Vector3 to, bool inverse)
            {
                return inverse ? (from - to).normalized : (to - from).normalized;
            }
            public static Vector2 DirectionTo(Vector2 from, Vector2 to)
            {
                return (to - from).normalized;
            }
            public static Vector2 Direction(Vector2 from, Vector2 to, bool inverse)
            {
                return inverse ? (from - to).normalized : (to - from).normalized;
            }
            public static Vector3 Reflect(Vector3 vector, Vector3 normal)
            {
                return vector - 2 * Vector3.Dot(vector, normal) * normal;
            }
            public static Vector3 Movealong(Vector3 vector, Vector3 normal)
            {
                return vector - Vector3.Dot(vector, normal) * normal;
            }
#endregion Direction
#region Random
            public static Vector3 RandomVector()
            {
                return Random.insideUnitCircle.normalized;
            }
            public static Vector3 RandomVector(Vector3 vector, float x = -1, float y = -1, float z = -1)
            {
                return new Vector3(x == -1 ? Random.value : vector.x, y == -1 ? Random.value : vector.y, z == -1 ? Random.value : vector.z);
            }
            public static Vector3 RandomVector(Vector3 vector, float maxPercDiff)
            {
                Vector3 r = Random.insideUnitSphere.normalized;
                float diff = Vector3.Angle(vector, r) / 180; // perc diff
                float res = Mathf.Max(0, diff - maxPercDiff); // perc diff that matters
                res = 1 - res; // perc to apply to new to get closer to old
                return Vector3.Lerp(r, vector, res).normalized; // a + (b -a ) * x
            }
            public static Vector3 RandomVectorInBounds(Vector3 vector, float maxPercDiff, Vector3 position, Bounds bounds)
            {
                Vector3 r = position.Direction(RandomPointInBounds(bounds));
                float diff = Vector3.Angle(vector, r) / 180; // perc diff
                float res = Mathf.Max(0, diff - maxPercDiff); // perc diff that matters
                return Vector3.Lerp(r, vector, res).normalized; // a + (b -a ) * x
            }
            public static Vector3 RandomPointInBounds(Bounds bounds)
            {
                return new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    Random.Range(bounds.min.z, bounds.max.z)
                );
            }
#endregion Random
#endregion Vector3 Additions
#region Vector2 Additions
            public static Vector2 Reflect(Vector2 vector, Vector2 normal)
            {
                return vector - 2 * Vector2.Dot(vector, normal) * normal;
            }
            public static Vector2 Movealong(Vector2 vector, Vector2 normal)
            {
                return vector - Vector2.Dot(vector, normal) * normal;
            }
#endregion Vector2 Additions
#region Async
            public delegate void Call();
            public static async Task DelayedCall(int delayms, Call call)
            {
                await Task.Delay(delayms);
                call.Invoke();
            }
            public static async Task MoveTo(Transform transform, Vector3 destination, float speed = 1f)
            {
                Vector3 basePosition = transform.position;
                float max = speed * 200; // 1000 (1s) / 5
                for (int i = 0; i <= max; i++)
                {
                    transform.position = Vector3.Lerp(basePosition, destination, i / max);
                    await Task.Delay(5);
                }
                return;
            }
            public static async Task MoveTo(Transform transform, Vector3 destination, Quaternion destinationRotation, float speed = 1f)
            {
                Quaternion baseRotation = transform.rotation;
                Vector3 basePosition = transform.position;
                float max = speed * 200; // 1000 (1s) / 5
                for (int i = 0; i <= max; i++)
                {
                    transform.position = Vector3.Lerp(basePosition, destination, i / max);
                    transform.rotation = Quaternion.Lerp(baseRotation, destinationRotation, i / max);
                    await Task.Delay(5);
                }
                return;
            }
            public static async Task MoveTo(Transform transform, Vector3[] points, float speed = 1f)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    await MoveTo(transform, points[i], speed);
                }
                return;
            }
            public static async Task MoveTo(Transform transform, Vector3[] points, Quaternion[] rotations, float speed = 1f)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    await MoveTo(transform, points[i], rotations[i], speed);
                }
                return;
            }
#endregion Async
        }
#region Timer
        public class RandomC
        {
            public static int RandomInt()
            {
                var rand = new System.Random();
                return rand.Next();
            }
            public static float RandomFloat()
            {
                var rand = new System.Random();
                return (float)rand.NextDouble();
            }
            public static double RandomDouble()
            {
                var rand = new System.Random();
                return rand.NextDouble();
            }
            public static int RandomInt(int max)
            {
                var rand = new System.Random();
                return rand.Next(max+1);
            }
            public static float RandomFloat(float max)
            {
                var rand = new System.Random();
                return (float)rand.NextDouble() * max;
            }
            public static double RandomDouble(double max)
            {
                var rand = new System.Random();
                return rand.NextDouble() * max;
            }
            public static int RandomInt(int min, int max)
            {
                var rand = new System.Random();
                return rand.Next(min, max+1);
            }
            public static float RandomFloat(float min, float max)
            {
                var rand = new System.Random();
                return (float)rand.NextDouble() * (max-min) + min;
            }
            public static double RandomDouble(double min, double max)
            {
                var rand = new System.Random();
                return rand.NextDouble() * (max-min) + min;
            }
        }
        public class Lerper
        {
            private async static void InvokeNow<T>(Action<T> action, T value, bool standardInvoke)
            {
                await Task.Delay(1);
                if (!standardInvoke)
                {
                    _ = action.BeginInvoke(value, action.EndInvoke, null);
                }
                else
                {
                    action.Invoke(value);
                }
            }
#region SetAtTimer
            private static float SetAtTimer(float origin, float target, float progress)
            {
                return Mathf.Lerp(origin, target, progress);
            }
            private static Vector2 SetAtTimer(Vector2 origin, Vector2 target, float progress)
            {
                return Vector2.Lerp(origin, target, progress);
            }
            private static Vector3 SetAtTimer(Vector3 origin, Vector3 target, float progress)
            {
                return Vector3.Lerp(origin, target, progress);
            }
            private static Quaternion SetAtTimer(Quaternion origin, Quaternion target, float progress)
            {
                return Quaternion.Lerp(origin, target, progress);
            }
#endregion SetAtTimer
#region float
            private static unsafe void SetUnsafe(float* source, float value)
            {
                *source = value;
            }

            public static void ConstantLerp(ref float origin, float destination, float duration)
            {
                unsafe
                {
                    fixed (float* p = &origin)
                        ConstantLerpUnsafe(duration, p, destination);
                }
            }

            public static void Lerp(ref float origin, float destination, float speed, float tolerance = 0.01f)
            {
                unsafe
                {
                    fixed (float* p = &origin)
                        LerpUnsafe(p, destination, speed, tolerance);
                }
            }


            public static void ConstantLerp(float origin, float destination, float duration, Action<float> setter)
            {

                ConstantLerpSafe(duration, origin, destination, setter);
            }

            public static void Lerp(float origin, float destination, float speed, Action<float> setter, float tolerance = 0.01f)
            {

                LerpSafe(origin, destination, speed, tolerance, setter);
            }

            private static void ConstantLerpSafe(float duration, float origin, float destination, Action<float> setter)
            {
                HandleConstantLerp(duration, origin, destination, setter, true);
            }

            private static void LerpSafe(float origin, float destination, float speed, float tolerance, Action<float> setter)
            {
                HandleLerp(origin, destination, tolerance, speed, setter, true);
            }


            private static unsafe void ConstantLerpUnsafe(float duration, float* origin, float destination)
            {
                HandleConstantLerp(duration, *origin, destination, (float param) => { SetUnsafe(origin, param); });
            }
            private static unsafe void LerpUnsafe(float* origin, float destination, float speed, float tolerance)
            {
                HandleLerp(*origin, destination, tolerance, speed, (float param) => { SetUnsafe(origin, param); });
            }

            private async static void HandleLerp(float origin, float destination, float tolerance, float speed, Action<float> setter, bool standardInvoke = false)
            {
                var r = new Stopwatch();
                r.Start();
                long prev = 0;
                await Task.Delay(5);
                while ((origin - destination).Abs() > tolerance)
                {
                    origin = Mathf.Lerp(origin, destination, (r.ElapsedMilliseconds - prev) / 1000f * speed);
                    prev = r.ElapsedMilliseconds;
                    InvokeNow(setter, origin, standardInvoke);
                    await Task.Delay(5);
                }
                setter.Invoke(destination);
            }

            private async static void HandleConstantLerp(float duration, float origin, float destination, Action<float> setter, bool standardInvoke = false)
            {
                var r = new Stopwatch();
                r.Start();
                float max = duration * 1000;
                while (r.ElapsedMilliseconds <= max)
                {
                    InvokeNow(setter, Mathf.Lerp(origin, destination, r.ElapsedMilliseconds / max), standardInvoke);
                    await Task.Delay(5);
                }
                setter.Invoke(destination);
            }

#endregion float
#region vector
            private static unsafe void SetUnsafe(Vector3* source, Vector3 value)
            {
                *source = value;
            }

            public static void ConstantLerp(ref Vector3 origin, Vector3 destination, float duration)
            {
                unsafe
                {
                    fixed (Vector3* p = &origin)
                        ConstantLerpUnsafe(duration, p, destination);
                }
            }

            public static void Lerp(ref Vector3 origin, Vector3 destination, float speed, float tolerance = 0.01f)
            {
                unsafe
                {
                    fixed (Vector3* p = &origin)
                        LerpUnsafe(p, destination, speed, tolerance);
                }
            }

            public static void ConstantLerp(Vector3 origin, Vector3 destination, float duration, Action<Vector3> setter)
            {
                
                ConstantLerpSafe(duration, origin, destination, setter);
            }

            public static void Lerp(Vector3 origin, Vector3 destination, float speed, Action<Vector3> setter, float tolerance = 0.01f)
            {
                
                LerpSafe(origin, destination, speed, tolerance, setter);
            }

            private static void ConstantLerpSafe(float duration, Vector3 origin, Vector3 destination, Action<Vector3> setter)
            {
                HandleConstantLerp(duration, origin, destination, setter, true);
            }

            private static void LerpSafe(Vector3 origin, Vector3 destination, float speed, float tolerance, Action<Vector3> setter)
            {
                HandleLerp(origin, destination, tolerance, speed, setter, true);
            }


            private static unsafe void ConstantLerpUnsafe(float duration, Vector3* origin, Vector3 destination)
            {
                HandleConstantLerp(duration, *origin, destination, (Vector3 param) => { SetUnsafe(origin, param); });
            }

            private static unsafe void LerpUnsafe(Vector3* origin, Vector3 destination, float speed, float tolerance)
            {
                HandleLerp(*origin, destination, tolerance, speed, (Vector3 param) => { SetUnsafe(origin, param); });
            }

            private async static void HandleLerp(Vector3 origin, Vector3 destination, float tolerance, float speed, Action<Vector3> setter, bool standardInvoke = false)
            {
                var r = new Stopwatch();
                r.Start();
                long prev = 0;
                await Task.Delay(5);
                while (origin.Distance(destination) > tolerance)
                {
                    origin = Vector3.Lerp(origin, destination, (r.ElapsedMilliseconds - prev) / 1000f * speed);
                    prev = r.ElapsedMilliseconds;
                    InvokeNow(setter, origin, standardInvoke);
                    await Task.Delay(5);
                }
                setter.Invoke(destination);
            }

            private async static void HandleConstantLerp(float duration, Vector3 origin, Vector3 destination, Action<Vector3> setter, bool standardInvoke = false)
            {
                var r = new Stopwatch();
                r.Start();
                float max = duration * 1000;
                while (r.ElapsedMilliseconds <= max)
                {
                    InvokeNow(setter, Vector3.Lerp(origin, destination, r.ElapsedMilliseconds / max), standardInvoke);
                    await Task.Delay(5);
                }
                setter.Invoke(destination);
            }
#endregion vector
#region vector2
            private static unsafe void SetUnsafe(Vector2* source, Vector2 value)
            {
                *source = value;
            }

            public static void ConstantLerp(ref Vector2 origin, Vector2 destination, float duration)
            {
                unsafe
                {
                    fixed (Vector2* p = &origin)
                        ConstantLerpUnsafe(duration, p, destination);
                }
            }

            public static void Lerp(ref Vector2 origin, Vector2 destination, float speed, float tolerance = 0.01f)
            {
                unsafe
                {
                    fixed (Vector2* p = &origin)
                        LerpUnsafe(p, destination, speed, tolerance);
                }
            }


            public static void ConstantLerp(Vector2 origin, Vector2 destination, float duration, Action<Vector2> setter)
            {

                ConstantLerpSafe(duration, origin, destination, setter);
            }

            public static void Lerp(Vector2 origin, Vector2 destination, float speed, Action<Vector2> setter, float tolerance = 0.01f)
            {

                LerpSafe(origin, destination, speed, tolerance, setter);
            }

            private static void ConstantLerpSafe(float duration, Vector2 origin, Vector2 destination, Action<Vector2> setter)
            {
                HandleConstantLerp(duration, origin, destination, setter, true);
            }

            private static void LerpSafe(Vector2 origin, Vector2 destination, float speed, float tolerance, Action<Vector2> setter)
            {
                HandleLerp(origin, destination, tolerance, speed, setter, true);
            }


            private static unsafe void ConstantLerpUnsafe(float duration, Vector2* origin, Vector2 destination)
            {
                HandleConstantLerp(duration, *origin, destination, (Vector2 param) => { SetUnsafe(origin, param); });
            }

            private static unsafe void LerpUnsafe(Vector2* origin, Vector2 destination, float speed, float tolerance)
            {
                HandleLerp(*origin, destination, tolerance, speed, (Vector2 param) => { SetUnsafe(origin, param); });
            }

            private async static void HandleLerp(Vector2 origin, Vector2 destination, float tolerance, float speed, Action<Vector2> setter, bool standardInvoke = false)
            {
                var r = new Stopwatch();
                r.Start();
                long prev = 0;
                await Task.Delay(5);
                while (origin.Distance(destination) > tolerance)
                {
                    origin = Vector2.Lerp(origin, destination, (r.ElapsedMilliseconds - prev) / 1000f * speed);
                    prev = r.ElapsedMilliseconds;
                    InvokeNow(setter, origin, standardInvoke);
                    await Task.Delay(5);
                }
                setter.Invoke(destination);
            }

            private async static void HandleConstantLerp(float duration, Vector2 origin, Vector2 destination, Action<Vector2> setter, bool standardInvoke = false)
            {
                var r = new Stopwatch();
                r.Start();
                float max = duration * 1000;
                while (r.ElapsedMilliseconds <= max)
                {
                    InvokeNow(setter, Vector2.Lerp(origin, destination, r.ElapsedMilliseconds / max), standardInvoke);
                    await Task.Delay(5);
                }
                setter.Invoke(destination);
            }
#endregion vector2
#region quaternion
            private static unsafe void SetUnsafe(Quaternion* source, Quaternion value)
            {
                *source = value;
            }

            public static void ConstantLerp(ref Quaternion origin, Quaternion destination, float duration)
            {
                unsafe
                {
                    fixed (Quaternion* p = &origin)
                        ConstantLerpUnsafe(duration, p, destination);
                }
            }

            public static void Lerp(ref Quaternion origin, Quaternion destination, float speed, float tolerance = 0.01f)
            {
                unsafe
                {
                    fixed (Quaternion* p = &origin)
                        LerpUnsafe(p, destination, speed, tolerance);
                }
            }

            public static void ConstantLerp(Quaternion origin, Quaternion destination, float duration, Action<Quaternion> setter)
            {

                ConstantLerpSafe(duration, origin, destination, setter);
            }

            public static void Lerp(Quaternion origin, Quaternion destination, float speed, Action<Quaternion> setter, float tolerance = 0.01f)
            {

                LerpSafe(origin, destination, speed, tolerance, setter);
            }

            private static void ConstantLerpSafe(float duration, Quaternion origin, Quaternion destination, Action<Quaternion> setter)
            {
                HandleConstantLerp(duration, origin, destination, setter, true);
            }

            private static void LerpSafe(Quaternion origin, Quaternion destination, float speed, float tolerance, Action<Quaternion> setter)
            {
                HandleLerp(origin, destination, tolerance, speed, setter, true);
            }


            private static unsafe void ConstantLerpUnsafe(float duration, Quaternion* origin, Quaternion destination)
            {
                HandleConstantLerp(duration, *origin, destination, (Quaternion param) => { SetUnsafe(origin, param); });
            }

            private static unsafe void LerpUnsafe(Quaternion* origin, Quaternion destination, float speed, float tolerance)
            {
                HandleLerp(*origin, destination, tolerance, speed, (Quaternion param) => { SetUnsafe(origin, param); });
            }

            private async static void HandleLerp(Quaternion origin, Quaternion destination, float tolerance, float speed, Action<Quaternion> setter, bool standardInvoke = false)
            {
                var r = new Stopwatch();
                r.Start();
                long prev = 0;
                await Task.Delay(5);
                while (!origin.IsApproximatelyEqual(destination, tolerance))
                {
                    origin = Quaternion.Lerp(origin, destination, (r.ElapsedMilliseconds - prev) / 1000f * speed);
                    prev = r.ElapsedMilliseconds;
                    InvokeNow(setter, origin, standardInvoke);
                    await Task.Delay(5);
                }
                setter.Invoke(destination);
            }

            private async static void HandleConstantLerp(float duration, Quaternion origin, Quaternion destination, Action<Quaternion> setter, bool standardInvoke = false)
            {
                var r = new Stopwatch();
                r.Start();
                float max = duration * 1000;
                while (r.ElapsedMilliseconds <= max)
                {
                    InvokeNow(setter, Quaternion.Lerp(origin, destination, r.ElapsedMilliseconds / max), standardInvoke);
                    await Task.Delay(5);
                }
                setter.Invoke(destination);
            }
#endregion quaternion
        }
        public class Timer
        {
            
            public static bool MinimumDelay(int id, int duration, bool firstTryPasses = false, bool clearOnSuccess = true)
            {
                var c = timers.FindIndex(t => t.Key == id);
                if (c != -1)
                {
                    var t = timers.Find(t => t.Key == id);
                    if (t.Value.ElapsedMilliseconds > duration)
                    {

                        if (clearOnSuccess)
                        {
                            t.Value.Stop();
                            timers.RemoveAt(c);
                        }
                        else
                        {
                            t.Value.Restart();
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    Stopwatch r = new Stopwatch();
                    r.Start();
                    timers.Add(new KeyValuePair<int, Stopwatch>(id, r));
                    if (duration == 0)
                        return true;
                    return firstTryPasses;

                }
            }
            public static void ClearDelay(int id)
            {
                var c = timers.FindIndex(t => t.Key == id);
                if (c != -1)
                {
                    var t = timers.Find(t => t.Key == id);
                    t.Value.Stop();
                    timers.RemoveAt(c);
                }
            }
            public delegate void TimerCallback();
            private static readonly List<KeyValuePair<int, Stopwatch>> timers = new List<KeyValuePair<int, Stopwatch>>();
            public static void StartTimer(int id)
            {
                Stopwatch r = new Stopwatch();
                r.Start();
                timers.Add(new KeyValuePair<int, Stopwatch>(id, r));
            }
            public static void StartTimer(int id, int delay, bool clearOnSuccess = true)
            {
                Stopwatch r = new Stopwatch();
                r.Start();
                timers.Add(new KeyValuePair<int, Stopwatch>(id, r));
                if (clearOnSuccess)
                    HandleClear(r, delay);
            }
            public static void StartTimer(int id, int delay, TimerCallback callback, bool loop = true)
            {
                Stopwatch r = new Stopwatch();
                r.Start();
                timers.Add(new KeyValuePair<int, Stopwatch>(id, r));
                HandleCallback(r, delay, callback, loop);
            }
            public static void StartTimer<T>(int id, int delay, Action<T> callback, T value, bool loop = true)
            {
                Stopwatch r = new Stopwatch();
                r.Start();
                timers.Add(new KeyValuePair<int, Stopwatch>(id, r));
                HandleCallback(r, delay, callback, value, loop);
            }
            public static void CollectGarbage(GameObject gameObject, int delay)
            {
                Action<GameObject> destroy = (g) => { UnityEngine.Object.Destroy(g); };
                StartTimer(gameObject.GetInstanceID(), delay, destroy, gameObject, false);
            }
            private async static void HandleClear(Stopwatch stopwatch, int delay)
            {
                while (stopwatch.IsRunning)
                {
                    await Task.Delay(5);
                    if (stopwatch.ElapsedMilliseconds > delay && stopwatch.IsRunning)
                    {
                        stopwatch.Stop();
                        timers.Remove(timers.Find(t => t.Value == stopwatch));
                        break;
                    }
                }
            }
            private async static void HandleCallback(Stopwatch stopwatch, int delay, TimerCallback callback, bool loop)
            {
                int objectiveDelay = delay;
                while (stopwatch.IsRunning)
                {
                    await Task.Delay(5);
                    if (stopwatch.ElapsedMilliseconds < objectiveDelay)
                        continue;
                    if (stopwatch.IsRunning)
                    {
                        callback.Invoke();
                        if (!loop)
                        {
                            stopwatch.Stop();
                            timers.Remove(timers.Find(t => t.Value == stopwatch));
                            break;
                        }
                        objectiveDelay += delay;
                    }
                }
            }
            private async static void HandleCallback<T>(Stopwatch stopwatch, int delay, Action<T> callback, T value, bool loop)
            {
                int objectiveDelay = delay;
                while (stopwatch.IsRunning)
                {
                    await Task.Delay(5);
                    if (stopwatch.ElapsedMilliseconds < objectiveDelay)
                        continue;
                    if (stopwatch.IsRunning)
                    {
                        callback.Invoke(value);
                        if (!loop)
                        {
                            stopwatch.Stop();
                            timers.Remove(timers.Find(t => t.Value == stopwatch));
                            break;
                        }
                        objectiveDelay += delay;
                    }
                }
            }
            public static void RestartTimer(int id)
            {
                timers.Find(t => t.Key == id).Value.Restart();
            }
            public static bool Exists(int id)
            {
                return timers.FindIndex(t => t.Key == id) != -1;
            }
            public static long GetTime(int id)
            {
                return timers.Find(t => t.Key == id).Value.ElapsedMilliseconds;
            }
            public static Stopwatch GetTimer(int id)
            {
                return timers.Find(t => t.Key == id).Value;
            }
            public static void EndTimer(int id)
            {
                var r = timers.Find(t => t.Key == id);
                if (!r.Equals(default(KeyValuePair<int, Stopwatch>)))
                {
                    r.Value.Stop();
                    timers.RemoveAll(t => t.Key == id);
                }
            }
            public static void StopTimer(int id)
            {
                EndTimer(id);
            }
        }
#endregion Timer
#endregion Utilities
    }
#region Definitions
    #region Serializable Quaternion
    [Serializable]
    public class SerializableQuaternion
    {
        [SerializeField]
        private SerializableVector3 _angle = new SerializableVector3();
        public Vector3 Angle
        {
            get
            {
                return _angle.Value;
            }
            set
            {
                _angle = new SerializableVector3(value);
                _q = Quaternion.Euler(value);
            }
        }
        private Quaternion _q = Quaternion.identity;
        public Quaternion Value
        {
            get
            {
                return _q;
            }
            set
            {
                _q = value;
                _angle = new SerializableVector3(value.eulerAngles);
            }
        }

        public SerializableQuaternion(Vector3 angle)
        {
            _q = Quaternion.Euler(angle);
            _angle = new SerializableVector3(angle);
        }

        public SerializableQuaternion(Quaternion angle)
        {
            _q = angle;
            _angle = new SerializableVector3(angle.eulerAngles);
        }

        public SerializableQuaternion()
        {
            _q = Quaternion.identity;
            _angle = new SerializableVector3(Quaternion.identity.eulerAngles);
        }

        public void Update()
        {
            _q = Quaternion.Euler(Angle);
        }
    }
    #endregion Serializable Quaternion
    #region Serializable Vector3
    [Serializable]
    public class SerializableVector3
    {
        [SerializeField]
        private float _x;
        [SerializeField]
        private float _y;
        [SerializeField]
        private float _z;

        public float x => _x;

        public float y => _y;

        public float z => _z;

        public Vector3 Value
        {
            get
            {
                return new Vector3(_x, _y, _z);
            }
            set
            {
                _x = value.x;
                _y = value.y;
                _z = value.z;
            }
        }

        public SerializableVector3(Vector3 value)
        {
            _x = value.x;
            _y = value.y;
            _z = value.z;
        }

        public SerializableVector3(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public SerializableVector3(SerializableVector3 value)
        {
            _x = value._x;
            _y = value._y;
            _z = value._z;
        }

        public SerializableVector3()
        {
            _x = 0;
            _y = 0;
            _z = 0;
        }
    }
    #endregion Serializable Vector3
    #region Serializable Dictionary
    // https://stackoverflow.com/a/1728996
    [Serializable]
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public SerializableDictionary(int capacity) : base(capacity) { }
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
    #endregion Serializable Dictionary
    #endregion Definitions
}