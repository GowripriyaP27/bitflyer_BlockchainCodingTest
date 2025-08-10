
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MerkleTreeLibrary
{
    /// <summary>
    /// Implements BIP340-compatible tagged hashing.
    /// </summary>
    public static class TaggedHash
    {
        public static byte[] Compute(string tag, byte[] message)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] tagHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tag));
                byte[] input = tagHash.Concat(tagHash).Concat(message).ToArray();
                return sha256.ComputeHash(input);
            }
        }
    }

    /// <summary>
    /// Represents a Merkle Tree and provides functionality to compute the Merkle Root.
    /// </summary>
    public class MerkleTree
    {
        private readonly string _tag;

        public MerkleTree(string tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Computes the Merkle Root from a list of string leaves.
        /// </summary>
        public byte[] ComputeMerkleRoot(List<string> leaves)
        {
            if (leaves == null || leaves.Count == 0)
                throw new ArgumentException("Leaf list must not be empty.");

            List<byte[]> hashedLeaves = leaves.Select(leaf =>
                TaggedHash.Compute(_tag, Encoding.UTF8.GetBytes(leaf))).ToList();

            return BuildTree(hashedLeaves);
        }

        /// <summary>
        /// Recursively builds the Merkle Tree and returns the root hash.
        /// </summary>
        private byte[] BuildTree(List<byte[]> nodes)
        {
            if (nodes.Count == 1)
                return nodes[0];

            List<byte[]> parentLevel = new List<byte[]>();

            for (int i = 0; i < nodes.Count; i += 2)
            {
                byte[] left = nodes[i];
                byte[] right = (i + 1 < nodes.Count) ? nodes[i + 1] : nodes[i]; // duplicate last if odd

                byte[] combined = left.Concat(right).ToArray();
                parentLevel.Add(TaggedHash.Compute(_tag, combined));
            }

            return BuildTree(parentLevel);
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string.
        /// </summary>
        public static string ToHex(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>
    /// Test class to demonstrate Merkle Tree functionality.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            List<string> input = new List<string> { "aaa", "bbb", "ccc", "ddd", "eee" };
            MerkleTree tree = new MerkleTree("Bitcoin_Transaction");

            byte[] root = tree.ComputeMerkleRoot(input);
            string hexRoot = MerkleTree.ToHex(root);

            Console.WriteLine("Merkle Root: " + hexRoot);
        }
    }
}
