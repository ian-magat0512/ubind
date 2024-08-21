// <copyright file="DkimHeaderVerifier.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using MimeKit;
    using MimeKit.Cryptography;

    /// <summary>
    /// This class is used to verify DKIM header.
    /// </summary>
    public class DkimHeaderVerifier : IDkimHeaderVerifier
    {
        public bool Verify(MimeMessage mimeMessage)
        {
            var publicKeyLocator = new DkimPublicKeyLocator();
            var verifier = new DkimVerifier(publicKeyLocator);
            var dkimSignatureIndex = mimeMessage.Headers.IndexOf(HeaderId.DkimSignature);
            if (dkimSignatureIndex == -1)
            {
                return false;
            }

            var dkimHeader = mimeMessage.Headers[dkimSignatureIndex];
            return verifier.Verify(mimeMessage, dkimHeader);
        }

        public void RemoveDkimSignature(MimeMessage mimeMessage)
        {
            if (mimeMessage.Headers.Contains(HeaderId.DkimSignature))
            {
                mimeMessage.Headers.Remove(HeaderId.DkimSignature);
            }
        }
    }
}
