// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("G8eNZWSWtxxzJ66vmpgkfq38uG8cGQqjcZa5Vb+BI9NW+D9eJPXb6Kwenb6skZqVthrUGmuRnZ2dmZyf0cPGRkQwupMmcM/hIi/GZyjuQaXNWkH7A4+ufh0ut+Q10YLfxdDXJbsHEW30+EaXGRe2Tj+dy8kG3fH329UYg/wQ/LjVlRycnkql3rAuWcRBySzSHfhaZBfW/NLpYbqEUa4NUy4I3ilcPRZX/0AlB8F9/rKKxlybfU+SrbFP9s/KwyVWTiRYG/focWIenZOcrB6dlp4enZ2cP3U8Uazb/z6OHoz64JIItWI2MKWJFRDgNuUm5Tiu61VKt+Pnke7O5XUQ7tvtfxgkCkx6CMn5AZq4MtDeoYzZ4YYTQAWynLdytm9YaZ6fnZyd");
        private static int[] order = new int[] { 3,10,3,6,7,7,6,8,8,12,10,13,13,13,14 };
        private static int key = 156;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
