using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Defines extension methods for classes that implement the <see cref="IResourceLoader"/> interface.
    /// </summary>
    public static class IResourceLoaderExtensions
    {
#if WINDOWS10_0_19041_0_OR_GREATER || NET472_OR_GREATER
        /// <summary>
        /// Gets an icon resource. Throws <see cref="ResourceNotFoundException"/> if not found.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <param name="ldr">The resource loader to get the resource from.</param>
        /// <returns>The icon resource defined by <paramref name="Name"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        public static System.Drawing.Icon GetIconResource(this IResourceLoader ldr, System.String Name)
            => ldr.GetResource<System.Drawing.Icon>(Name);

        /// <summary>
        /// Gets a bitmap resource. Throws <see cref="ResourceNotFoundException"/> if not found.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <param name="ldr">The resource loader to get the resource from.</param>
        /// <returns>The bitmap resource defined by <paramref name="Name"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        public static System.Drawing.Bitmap GetBitmapResource(this IResourceLoader ldr, System.String Name)
            => ldr.GetResource<System.Drawing.Bitmap>(Name);

        /// <summary>
        /// Gets an image resource. The image provided can be any valid image object that is either <see cref="System.Drawing.Icon"/> , 
        /// <see cref="System.Drawing.Bitmap"/> , or <see cref="System.Drawing.Imaging.Metafile"/> .
        /// </summary>
        /// <param name="ldr">The resource loader to get the resource from.</param>
        /// <param name="Name">The resource name to look up.</param>
        /// <returns>The image resource defined by <paramref name="Name"/>.</returns>
        /// <exception cref="ResourceTypeMismatchException">The specified image was not any of 
        /// <see cref="System.Drawing.Icon"/> , <see cref="System.Drawing.Bitmap"/> or  <see cref="System.Drawing.Imaging.Metafile"/>.</exception>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        public static System.Drawing.Image GetImageResource(this IResourceLoader ldr , System.String Name)
        {
            System.Object obj = ldr.GetResource(Name);
            if (obj is System.Drawing.Bitmap || obj is System.Drawing.Imaging.Metafile) {
               return (System.Drawing.Image)obj;
            }
            if (obj is System.Drawing.Icon ic) {
                return ic.ToBitmap();
            }
            throw new ResourceTypeMismatchException(obj.GetType(), typeof(System.Drawing.Image), Name);
        }

        /// <summary>
        /// Gets an image resource specified by <paramref name="Name"/> parameter and converts the image
        /// to a suitable XAML image for loading it and using it in a WPF application environment.
        /// </summary>
        /// <param name="ldr">The resource loader to get the resource from.</param>
        /// <param name="Name">The resource name to look up.</param>
        /// <returns></returns>
        /// <exception cref = "ResourceTypeMismatchException" > The specified image was not any of
        /// <see cref="System.Drawing.Icon"/> , <see cref="System.Drawing.Bitmap"/> or  <see cref="System.Drawing.Imaging.Metafile"/>.</exception>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        public static System.Windows.Media.Imaging.BitmapImage GetImageResourceAsWPF(this IResourceLoader ldr , System.String Name)
        {
            System.Drawing.Image img = GetImageResource(ldr, Name);
            System.IO.Stream STR = new System.IO.MemoryStream();
            System.Windows.Media.Imaging.BitmapImage IMG = new();
            try {
                img.Save(STR, img.RawFormat);
            } catch (ArgumentNullException) {
                img.Save(STR, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            STR.Position = 0;
            IMG.BeginInit();
            IMG.StreamSource = STR;
            IMG.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            IMG.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation;
            IMG.EndInit();
            IMG.Freeze();
            return IMG;
        }
#endif

        /// <summary>
        /// Gets the resource type for the specified resource.
        /// </summary>
        /// <param name="ldr">The loader to take information from.</param>
        /// <param name="name">The name of the resource to retrieve the type defined in the resource's value.</param>
        /// <returns>The type of the underlying resource value , if found; otherwise , <see langword="null"/>.</returns>
        public static System.Type GetResourceType(this IResourceLoader ldr , System.String name)
        {
            foreach (var entry in ldr)
            {
                if (entry.Name == name)
                {
                    return entry.TypeOfValue;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a byte array resource. Throws <see cref="ResourceNotFoundException"/> if not found.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <param name="ldr">The resource loader to get the resource from.</param>
        /// <returns>The byte array resource defined by <paramref name="Name"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        public static System.Byte[] GetByteArrayResource(this IResourceLoader ldr, System.String Name)
            => ldr.GetResource<System.Byte[]>(Name);

        /// <summary>
        /// Gets the first relative resource found. If the given sequence has less than 2 elements , 
        /// throws <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <typeparam name="T">The resource type to return.</typeparam>
        /// <param name="ldr">The resource loader to get the resource from.</param>
        /// <param name="LookupName">The name to search for relative names.</param>
        /// <returns>The first relative resource , casted to <typeparamref name="T"/>.</returns>
        public static T GetFirstRelativeResource<T>(this IResourceLoader ldr, System.String LookupName)
        {
            IEnumerable<System.String> di = ldr.GetRelativeResources(LookupName);
            return ldr.GetResource<T>(di.ElementAt(1));
        }

        /// <summary>
        /// Gets the last relative resource found.
        /// </summary>
        /// <typeparam name="T">The resource type to return.</typeparam>
        /// <param name="ldr">The resource loader to get the resource from.</param>
        /// <param name="LookupName">The name to search for relative names.</param>
        /// <returns>The first relative resource , casted to <typeparamref name="T"/>.</returns>
        public static T GetLastRelativeResource<T>(this IResourceLoader ldr, System.String LookupName)
        {
            IEnumerable<System.String> di = ldr.GetRelativeResources(LookupName);
            return ldr.GetResource<T>(di.Last());
        }

        /// <summary>
        /// Gets the first resource name defined in the resource data.
        /// </summary>
        /// <param name="ldr">The resource loader to get the name from.</param>
        /// <returns>The first resource name.</returns>
        public static System.String GetFirstResourceName(this IResourceLoader ldr)
        {
            IEnumerable<System.String> names = ldr.GetAllResourceNames();
            return names.First();
        }

        /// <summary>
        /// Gets the last resource name defined in the resource data.
        /// </summary>
        /// <param name="ldr">The resource loader to get the name from.</param>
        /// <returns>The last resource name.</returns>
        public static System.String GetLastResourceName(this IResourceLoader ldr)
        {
            IEnumerable<System.String> names = ldr.GetAllResourceNames();
            return names.Last();
        }

        /// <summary>
        /// Gets a resource boxed in a <see cref="Object"/> instance , from the specified loader.
        /// </summary>
        /// <param name="ldr">The resource loader instance.</param>
        /// <param name="Name">The resource name.</param>
        /// <param name="underlyingtype">The resource type to retrieve</param>
        /// <returns>The boxed resource value.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource name and type were not found.</exception>
        public static System.Object GetResource(this IResourceLoader ldr , System.String Name , System.Type underlyingtype)
        {
            foreach (var entry in ldr)
            {
                if (entry.Name == Name && underlyingtype == entry.TypeOfValue)
                {
                    return entry;
                }
            }
            throw new ResourceNotFoundException(Name);
        }

        /// <summary>
        /// Gets a resource boxed in a <see cref="Object"/> instance , from the specified loader.
        /// </summary>
        /// <param name="ldr">The resource loader instance.</param>
        /// <param name="Name">The resource name.</param>
        /// <returns>The boxed resource value.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource name was not found.</exception>
        public static System.Object GetResource(this IResourceLoader ldr , System.String Name)
        {
            foreach (var entry in ldr)
            {
                if (entry.Name == Name)
                {
                    return entry.Value;
                }
            }
            throw new ResourceNotFoundException(Name);
        }

    }
}
