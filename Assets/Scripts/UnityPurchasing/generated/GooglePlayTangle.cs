// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("+Xp0e0v5enF5+Xp6e9iS27ZLPBjD7aud7y4e5n1f1Tc5Rms+BmH0pwLfSQyyrVAEAHYJKQKS9wk8Cpj/pi7LNfofvYPwMRs1DoZdY7ZJ6rRc4PaKEx+hcP7wUanYeiwu4ToWEEv5ellLdn1yUf0z/Yx2enp6fnt4+/7tRJZxXrJYZsQ0sR/YucMSPA+aqHVKVqgRKC0kwrGpw7/8EA+WhfwgaoKDcVD7lMBJSH1/w5lKG1+Iye85zrva8bAYp8LgJpoZVW0hu3wqvaYc5GhJmfrJUAPSNmU4Ijcwwtlp+WsdB3XvUoXR10Ju8vcH0QLBPDL/ZBv3G18ycvt7ea1COVfJviM2JCGho9dddMGXKAbFyCGAzwmmQuJVe1CVUYi/jnl4ent6");
        private static int[] order = new int[] { 1,9,11,8,4,9,10,12,8,9,12,13,13,13,14 };
        private static int key = 123;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
