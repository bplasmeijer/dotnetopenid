﻿//-----------------------------------------------------------------------
// <copyright file="MockTransformationBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	internal class MockTransformationBindingElement : IChannelBindingElement {
		private string transform;

		internal MockTransformationBindingElement(string transform) {
			if (transform == null) {
				throw new ArgumentNullException("transform");
			}

			this.transform = transform;
		}

		#region IChannelBindingElement Members

		ChannelProtection IChannelBindingElement.Protection {
			get { return ChannelProtection.None; }
		}

		void IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			var testMessage = message as TestMessage;
			if (testMessage != null) {
				testMessage.Name = this.transform + testMessage.Name;
			}
		}

		void IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			var testMessage = message as TestMessage;
			if (testMessage != null) {
				StringAssert.StartsWith(testMessage.Name, this.transform);
				testMessage.Name = testMessage.Name.Substring(this.transform.Length);
			}
		}

		#endregion
	}
}