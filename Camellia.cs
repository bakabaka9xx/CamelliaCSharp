//
// Camellia.cs
//

using System;

namespace Camellia {
  class Camellia128 {
    private ulong[] eKey;
    private ulong[] S1, S2, S3, S4;

    public Camellia128(){
      eKey = new ulong[26];
      S1 = new ulong[256];
      S2 = new ulong[256];
      S3 = new ulong[256];
      S4 = new ulong[256];

      GenSbox();
    }

    private void GenSbox() {
      byte[] s = {
        112,130, 44,236,179, 39,192,229,228,133, 87, 53,234, 12,174, 65,
         35,239,107,147, 69, 25,165, 33,237, 14, 79, 78, 29,101,146,189,
        134,184,175,143,124,235, 31,206, 62, 48,220, 95, 94,197, 11, 26,
        166,225, 57,202,213, 71, 93, 61,217,  1, 90,214, 81, 86,108, 77,
        139, 13,154,102,251,204,176, 45,116, 18, 43, 32,240,177,132,153,
        223, 76,203,194, 52,126,118,  5,109,183,169, 49,209, 23,  4,215,
         20, 88, 58, 97,222, 27, 17, 28, 50, 15,156, 22, 83, 24,242, 34,
        254, 68,207,178,195,181,122,145, 36,  8,232,168, 96,252,105, 80,
        170,208,160,125,161,137, 98,151, 84, 91, 30,149,224,255,100,210,
         16,196,  0, 72,163,247,117,219,138,  3,230,218,  9, 63,221,148,
        135, 92,131,  2,205, 74,144, 51,115,103,246,243,157,127,191,226,
         82,155,216, 38,200, 55,198, 59,129,150,111, 75, 19,190, 99, 46,
        233,121,167,140,159,110,188,142, 41,245,249,182, 47,253,180, 89,
        120,152,  6,106,231, 70,113,186,212, 37,171, 66,136,162,141,250,
        114,  7,185, 85,248,238,172, 10, 54, 73, 42,104, 60, 56,241,164,
         64, 40,211,123,187,201, 67,193, 21,227,173,244,119,199,128,158
      };

      for(int i=0; i<256; i++) {
        ulong s1 = s[i];
        ulong s2 = ((s1<<1)|(s1>>7))&0xff;
        ulong s3 = ((s1<<7)|(s1>>1))&0xff;
        ulong s4 = s[((i<<1)|(i>>7))&0xff];
        S1[i] = S2[i] = S3[i] = S4[i] = 0;
        for(int j=0; j<64; j+=8){
          S1[i] |= (s1<<j);
          S2[i] |= (s2<<j);
          S3[i] |= (s3<<j);
          S4[i] |= (s4<<j);
        }
      }
    }

    private ulong F(ulong x, ulong k) {
      ulong y;
      ulong t = x^k;
      y  = S1[ (t    )&0xff ] & 0xffffff00ffffff00;
      y ^= S4[ (t>>=8)&0xff ] & 0xffff00ffffff00ff;
      y ^= S3[ (t>>=8)&0xff ] & 0xff00ffffff00ffff;
      y ^= S2[ (t>>=8)&0xff ] & 0x00ffffff00ffffff;
      y ^= S4[ (t>>=8)&0xff ] & 0xffff00ff0000ffff;
      y ^= S3[ (t>>=8)&0xff ] & 0xff00ffff00ffff00;
      y ^= S2[ (t>>=8)&0xff ] & 0x00ffffffffff0000;
      y ^= S1[ (t>>=8)&0xff ] & 0xffffff00ff0000ff;
      return y;
    }

    private ulong FL(ulong x, ulong k) {
      uint xl = (uint)(x>>32);
      uint xr = (uint)(x&0xffffffff);
      uint t = xl & (uint)(k>>32);
      xr ^= (t<<1)|(t>>31);
      xl ^= xr | (uint)(k&0xffffffff);
      return ((ulong)xl<<32)|xr;
    }

    private ulong FLi(ulong y, ulong k) {
      uint yl = (uint)(y>>32);
      uint yr = (uint)(y&0xffffffff);
      yl ^= yr | (uint)(k&0xffffffff);
      uint t = yl & (uint)(k>>32);
      yr ^= (t<<1)|(t>>31);
      return ((ulong)yl<<32)|yr;
    }

    private static ulong Bswap(ulong x) {
      ulong t;
      t = ((x&0x00ff00ff00ff00ff)<<8)|((x>>8)&0x00ff00ff00ff00ff);
      t = ((t&0x0000ffff0000ffff)<<16)|((t>>16)&0x0000ffff0000ffff);
      t = ((t&0x00000000ffffffff)<<32)|((t>>32)&0x00000000ffffffff);
      return t;
    }

    public bool SetKey(byte[] key) {
      if(key.Length!=16) return false;

      ulong KAl, KAr;
      ulong KLl = Bswap(BitConverter.ToUInt64(key, 0));
      ulong KLr = Bswap(BitConverter.ToUInt64(key, 8));

      eKey[0] = KLl;
      eKey[1] = KLr;
      eKey[4] = (KLl<<15)|(KLr>>49);
      eKey[5] = (KLr<<15)|(KLl>>49);
      eKey[10] = (KLl<<45)|(KLr>>19);
      eKey[11] = (KLr<<45)|(KLl>>19);
      eKey[13] = (KLr<<60)|(KLl>>4);
      eKey[16] = (KLr<<13)|(KLl>>51);
      eKey[17] = (KLl<<13)|(KLr>>51);
      eKey[18] = (KLr<<30)|(KLl>>34);
      eKey[19] = (KLl<<30)|(KLr>>34);
      eKey[22] = (KLr<<47)|(KLl>>17);
      eKey[23] = (KLl<<47)|(KLr>>17);

      KAr = F(KLl, 0xA09E667F3BCC908B);
      KAl = F(KAr^KLr, 0xB67AE8584CAA73B2);
      KAr ^= F(KAl, 0xC6EF372FE94F82BE);
      KAl ^= F(KAr, 0x54FF53A5F1D36F1C);

      eKey[2] = KAl;
      eKey[3] = KAr;
      eKey[6] = (KAl<<15)|(KAr>>49);
      eKey[7] = (KAr<<15)|(KAl>>49);
      eKey[8] = (KAl<<30)|(KAr>>34);
      eKey[9] = (KAr<<30)|(KAl>>34);
      eKey[12] = (KAl<<45)|(KAr>>19);
      eKey[14] = (KAl<<60)|(KAr>>4);
      eKey[15] = (KAr<<60)|(KAl>>4);
      eKey[20] = (KAr<<30)|(KAl>>34);
      eKey[21] = (KAl<<30)|(KAr>>34);
      eKey[24] = (KAr<<47)|(KAl>>17);
      eKey[25] = (KAl<<47)|(KAr>>17);

      return true;
    }

    public byte[] Encrypt(byte[] msg) {
      byte[] c = new byte[msg.Length];

      ulong Ml = Bswap(BitConverter.ToUInt64(msg, 0));
      ulong Mr = Bswap(BitConverter.ToUInt64(msg, 8));

      ulong Cr = Ml ^ eKey[0];
      ulong Cl = Mr ^ eKey[1];

      Cl ^= F(Cr, eKey[2]);
      Cr ^= F(Cl, eKey[3]);
      Cl ^= F(Cr, eKey[4]);
      Cr ^= F(Cl, eKey[5]);
      Cl ^= F(Cr, eKey[6]);
      Cr ^= F(Cl, eKey[7]);

      Cr = FL(Cr, eKey[8]);
      Cl = FLi(Cl, eKey[9]);

      Cl ^= F(Cr, eKey[10]);
      Cr ^= F(Cl, eKey[11]);
      Cl ^= F(Cr, eKey[12]);
      Cr ^= F(Cl, eKey[13]);
      Cl ^= F(Cr, eKey[14]);
      Cr ^= F(Cl, eKey[15]);

      Cr = FL(Cr, eKey[16]);
      Cl = FLi(Cl, eKey[17]);

      Cl ^= F(Cr, eKey[18]);
      Cr ^= F(Cl, eKey[19]);
      Cl ^= F(Cr, eKey[20]);
      Cr ^= F(Cl, eKey[21]);
      Cl ^= F(Cr, eKey[22]);
      Cr ^= F(Cl, eKey[23]);

      Cl ^= eKey[24];
      Cr ^= eKey[25];

      Buffer.BlockCopy(BitConverter.GetBytes(Bswap(Cl)), 0, c, 0, 8);
      Buffer.BlockCopy(BitConverter.GetBytes(Bswap(Cr)), 0, c, 8, 8);

      return c;
    }
  }
}
