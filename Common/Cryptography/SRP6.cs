﻿using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Common.Cryptography
{
    public class SRP6
    {
        public byte[] A;
        public byte[] b;
        public byte[] B;
        private IntPtr BNA;
        private IntPtr BNb;
        private IntPtr BNB;
        private IntPtr BNg;
        private IntPtr BNk;
        private IntPtr BNn;
        private IntPtr BNS;
        private IntPtr BNU;
        private IntPtr BNv;
        private IntPtr BNx;
        public byte[] g = new byte[] { 7 };
        private static HashAlgorithm hashAlgorithm = new SHA1Managed();
        public byte[] k;
        public byte[] K;
        public byte[] M2;
        public byte[] N;
        public byte[] Password;
        public byte[] S;
        public byte[] salt;
        public byte[] U;
        public byte[] Username;

        public SRP6()
        {
            N = new byte[] { 0x89, 0x4b, 100, 0x5e, 0x89, 0xe1, 0x53, 0x5b, 0xbd, 0xad, 0x5b, 0x8b, 0x29, 6, 80, 0x53,
                             8, 1, 0xb1, 0x8e, 0xbf, 0xbf, 0x5e, 0x8f, 0xab, 60, 130, 0x87, 0x2a, 0x3e, 0x9b, 0xb7 };

            salt = new byte[] { 0xad, 0xd0, 0x3a, 0x31, 210, 0x71, 20, 70, 0x75, 0xf2, 0x70, 0x7e, 80, 0x26, 0xb6, 210,
                                0xf1, 0x86, 0x59, 0x99, 0x76, 2, 80, 170, 0xb9, 0x45, 0xe0, 0x9e, 0xdd, 0x2a, 0xa3, 0x45 };

            k = new byte[] { 3 };
            B = new byte[0x20];
            b = new byte[20];
        }

        [DllImport("LIBEAY32.DLL")]
        private static extern int BN_add(IntPtr r, IntPtr a, IntPtr b);
        [DllImport("LIBEAY32.DLL", EntryPoint = "BN_bin2bn")]
        private static extern IntPtr BN_Bin2BN(byte[] ByteArrayIn, int length, IntPtr to);
        [DllImport("LIBEAY32.DLL")]
        private static extern int BN_bn2bin(IntPtr a, byte[] to);
        [DllImport("LIBEAY32.DLL", EntryPoint = "BN_CTX_free")]
        private static extern int BN_ctx_free(IntPtr a);
        [DllImport("LIBEAY32.DLL", EntryPoint = "BN_CTX_new")]
        private static extern IntPtr BN_ctx_new();
        [DllImport("LIBEAY32.DLL")]
        private static extern int BN_div(IntPtr dv, IntPtr r, IntPtr a, IntPtr b, IntPtr ctx);
        [DllImport("LIBEAY32.DLL", EntryPoint = "BN_free")]
        private static extern void BN_Free(IntPtr r);
        [DllImport("LIBEAY32.DLL")]
        private static extern IntPtr BN_mod_exp(IntPtr res, IntPtr a, IntPtr p, IntPtr m, IntPtr ctx);
        [DllImport("LIBEAY32.DLL")]
        private static extern int BN_mul(IntPtr r, IntPtr a, IntPtr b, IntPtr ctx);
        [DllImport("LIBEAY32.DLL", EntryPoint = "BN_new")]
        private static extern IntPtr BN_New();
        public void CalculateB()
        {
            RAND_bytes(this.b, 20);
            IntPtr res = BN_New();
            IntPtr r = BN_New();
            IntPtr ptr3 = BN_New();
            this.BNB = BN_New();
            IntPtr ctx = BN_ctx_new();
            Array.Reverse(this.b);
            this.BNb = BN_Bin2BN(this.b, this.b.Length, IntPtr.Zero);
            Array.Reverse(this.b);
            BN_mod_exp(res, this.BNg, this.BNb, this.BNn, ctx);
            BN_mul(r, this.BNk, this.BNv, ctx);
            BN_add(ptr3, res, r);
            BN_div(IntPtr.Zero, this.BNB, ptr3, this.BNn, ctx);
            BN_bn2bin(this.BNB, this.B);
            Array.Reverse(this.B);
            BN_ctx_free(ctx);
            BN_Free(ptr3);
            BN_Free(r);
            BN_Free(res);
        }

        public void CalculateK()
        {
            ArrayList list = Split(this.S);
            list[0] = hashAlgorithm.ComputeHash((byte[])list[0]);
            list[1] = hashAlgorithm.ComputeHash((byte[])list[1]);
            this.K = Combine((byte[])list[0], (byte[])list[1]);
        }

        public void CalculateM2(byte[] m1)
        {
            byte[] dst = new byte[(this.A.Length + m1.Length) + this.K.Length];
            Buffer.BlockCopy(this.A, 0, dst, 0, this.A.Length);
            Buffer.BlockCopy(m1, 0, dst, this.A.Length, m1.Length);
            Buffer.BlockCopy(this.K, 0, dst, this.A.Length + m1.Length, this.K.Length);
            this.M2 = hashAlgorithm.ComputeHash(dst);
        }

        public void CalculateS()
        {
            IntPtr res = BN_New();
            IntPtr r = BN_New();
            this.BNS = BN_New();
            IntPtr ctx = BN_ctx_new();
            this.S = new byte[0x20];
            BN_mod_exp(res, this.BNv, this.BNU, this.BNn, ctx);
            BN_mul(r, this.BNA, res, ctx);
            BN_mod_exp(this.BNS, r, this.BNb, this.BNn, ctx);
            BN_bn2bin(this.BNS, this.S);
            Array.Reverse(this.S);
            this.CalculateK();
            BN_ctx_free(ctx);
            BN_Free(r);
            BN_Free(res);
        }

        public void CalculateU(byte[] a)
        {
            this.A = a;
            byte[] dst = new byte[a.Length + this.B.Length];
            Buffer.BlockCopy(a, 0, dst, 0, a.Length);
            Buffer.BlockCopy(this.B, 0, dst, a.Length, this.B.Length);
            this.U = hashAlgorithm.ComputeHash(dst);
            Array.Reverse(this.U);
            this.BNU = BN_Bin2BN(this.U, this.U.Length, IntPtr.Zero);
            Array.Reverse(this.U);
            Array.Reverse(this.A);
            this.BNA = BN_Bin2BN(this.A, this.A.Length, IntPtr.Zero);
            Array.Reverse(this.A);
            this.CalculateS();
        }

        public void CalculateV()
        {
            this.BNv = BN_New();
            IntPtr ctx = BN_ctx_new();
            BN_mod_exp(this.BNv, this.BNg, this.BNx, this.BNn, ctx);
            this.CalculateB();
            BN_ctx_free(ctx);
        }

        public void CalculateX(byte[] username, byte[] password)
        {
            byte[] src = username;
            byte[] buffer2 = password;
            byte[] dst = new byte[(src.Length + buffer2.Length) + 1];
            byte[] buffer5 = new byte[this.salt.Length + 20];
            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            dst[src.Length] = 0x3a;
            Buffer.BlockCopy(buffer2, 0, dst, src.Length + 1, buffer2.Length);
            Buffer.BlockCopy(hashAlgorithm.ComputeHash(dst, 0, dst.Length), 0, buffer5, this.salt.Length, 20);
            Buffer.BlockCopy(this.salt, 0, buffer5, 0, this.salt.Length);
            byte[] array = hashAlgorithm.ComputeHash(buffer5);
            Array.Reverse(array);
            this.BNx = BN_Bin2BN(array, array.Length, IntPtr.Zero);
            Array.Reverse(this.g);
            this.BNg = BN_Bin2BN(this.g, this.g.Length, IntPtr.Zero);
            Array.Reverse(this.g);
            Array.Reverse(this.k);
            this.BNk = BN_Bin2BN(this.k, this.k.Length, IntPtr.Zero);
            Array.Reverse(this.k);
            Array.Reverse(this.N);
            this.BNn = BN_Bin2BN(this.N, this.N.Length, IntPtr.Zero);
            Array.Reverse(this.N);
            this.CalculateV();
        }

        private static byte[] Combine(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
            {
                return null;
            }
            byte[] buffer = new byte[b1.Length + b2.Length];
            int index = 0;
            int num2 = 1;
            for (int i = 0; i < b1.Length; i++)
            {
                buffer[index] = b1[i];
                index++;
                index++;
            }
            for (int j = 0; j < b2.Length; j++)
            {
                buffer[num2] = b2[j];
                num2++;
                num2++;
            }
            return buffer;
        }

        ~SRP6()
        {
            BN_Free(this.BNA);
            BN_Free(this.BNb);
            BN_Free(this.BNB);
            BN_Free(this.BNg);
            BN_Free(this.BNk);
            BN_Free(this.BNn);
            BN_Free(this.BNS);
            BN_Free(this.BNU);
            BN_Free(this.BNv);
            BN_Free(this.BNx);
        }

        [DllImport("LIBEAY32.DLL")]
        public static extern int RAND_bytes(byte[] buf, int num);
        private static ArrayList Split(byte[] bo)
        {
            byte[] dst = new byte[bo.Length - 1];
            if (((bo.Length % 2) != 0) && (bo.Length > 2))
            {
                Buffer.BlockCopy(bo, 1, dst, 0, bo.Length);
            }
            byte[] buffer2 = new byte[bo.Length / 2];
            byte[] buffer3 = new byte[bo.Length / 2];
            int index = 0;
            int num2 = 1;
            for (int i = 0; i < buffer2.Length; i++)
            {
                buffer2[i] = bo[index];
                index++;
                index++;
            }
            for (int j = 0; j < buffer3.Length; j++)
            {
                buffer3[j] = bo[num2];
                num2++;
                num2++;
            }
            ArrayList list = new ArrayList();
            list.Add(buffer2);
            list.Add(buffer3);
            return list;
        }
    }
}