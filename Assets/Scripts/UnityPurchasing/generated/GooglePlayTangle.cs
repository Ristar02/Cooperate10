// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("bGltw0Js/YN9WxsbeJqqLDhCGYSpKiQrG6kqISmpKior38YvCbxG/S1mIWRELGgdonwEOzBsh/gberQsbYovRt/lbyc5bqHlmmsER2IANo1iWBli5Nk+4RZ+Yakdc0w63lhVKg/KgqKBSzUwxkQ8NQ4R9YUpdLZNIaVzBOCRU7vb3krZ9vmaeL4A7B4uV7LNT+MRtGOJ2l2UTyyEdRxgeS51J0ux4XkXLayaG52nCu5uJcdijDlaIxnWbimn6YkWS42CcZBBa0ys71vTTvBQu8P0vrrE0wlXvDkMrBupKgkbJi0iAa1jrdwmKioqLisowV4foL9kJRqwLzPBBc1qLeeCpgvmoeI8W+f/eqESivkLgfU3wQEw0Gto159hnNtBlikoKisq");
        private static int[] order = new int[] { 6,1,9,11,4,8,7,12,8,13,10,12,13,13,14 };
        private static int key = 43;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
