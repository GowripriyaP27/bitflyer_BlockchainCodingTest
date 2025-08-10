
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MerkleTreeLibrary
{
    public static class TaggedHash
    {
        public static byte[] Compute(string tag, byte[] message)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] tagHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tag));
                byte[] combined = tagHash.Concat(tagHash).Concat(message).ToArray();
                return sha256.ComputeHash(combined);
            }
        }

        public static string ComputeHex(string tag, byte[] message)
        {
            return BitConverter.ToString(Compute(tag, message)).Replace("-", "").ToLower();
        }
    }

    public class MerkleTree
    {
        private const string HashTag = "Bitcoin_Transaction";

        public static byte[] ComputeMerkleRoot(List<string> leaves)
        {
            List<byte[]> hashedLeaves = leaves.Select(leaf =>
                TaggedHash.Compute(HashTag, Encoding.UTF8.GetBytes(leaf))
            ).ToList();

            while (hashedLeaves.Count > 1)
            {
                List<byte[]> newLevel = new List<byte[]>();
                for (int i = 0; i < hashedLeaves.Count; i += 2)
                {
                    if (i + 1 < hashedLeaves.Count)
                    {
                        byte[] combined = hashedLeaves[i].Concat(hashedLeaves[i + 1]).ToArray();
                        newLevel.Add(TaggedHash.Compute(HashTag, combined));
                    }
                    else
                    {
                        newLevel.Add(hashedLeaves[i]);
                    }
                }
                hashedLeaves = newLevel;
            }

            return hashedLeaves[0];
        }

        public static string ComputeMerkleRootHex(List<string> leaves)
        {
            return BitConverter.ToString(ComputeMerkleRoot(leaves)).Replace("-", "").ToLower();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<string> data = new List<string> { "aaa", "bbb", "ccc", "ddd", "eee" };
            string merkleRoot = MerkleTree.ComputeMerkleRootHex(data);
            Console.WriteLine("Merkle Root: " + merkleRoot);
        }
    }
}
