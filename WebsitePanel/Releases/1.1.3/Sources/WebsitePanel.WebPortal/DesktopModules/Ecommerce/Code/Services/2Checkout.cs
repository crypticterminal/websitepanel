// Copyright (c) 2010, SMB SAAS Systems Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must  retain  the  above copyright notice, this
//   list of conditions and the following disclaimer.
//
// - Redistributions in binary form  must  reproduce the  above  copyright  notice,
//   this list of conditions  and  the  following  disclaimer in  the documentation
//   and/or other materials provided with the distribution.
//
// - Neither  the  name  of  the  SMB SAAS Systems Inc.  nor   the   names  of  its
//   contributors may be used to endorse or  promote  products  derived  from  this
//   software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,  BUT  NOT  LIMITED TO, THE IMPLIED
// WARRANTIES  OF  MERCHANTABILITY   AND  FITNESS  FOR  A  PARTICULAR  PURPOSE  ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL,  SPECIAL,  EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO,  PROCUREMENT  OF  SUBSTITUTE  GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)  HOWEVER  CAUSED AND ON
// ANY  THEORY  OF  LIABILITY,  WHETHER  IN  CONTRACT,  STRICT  LIABILITY,  OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE)  ARISING  IN  ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace WebsitePanel.WebPortal.Services.Ecommerce
{
	public class _2Checkout : ServiceHandlerBase
	{
		public _2Checkout()
			: base("6A847B61-6178-445d-93FC-1929E86984DF", false)
		{
			PreProcessRequest += new EventHandler(ServiceHandler_PreProcessRequest);
			PostProcessRequest += new EventHandler(ServiceHandler_PostProcessRequest);
		}

		private void ServiceHandler_PreProcessRequest(object sender, EventArgs e)
		{
			HttpContext context = (HttpContext)sender;
			//
			SetProperty(CONTRACT_ID, context.Request.Form["contract_id"]);
			//
			SetProperty(INVOICE_ID, context.Request.Form["merchant_order_id"]);
		}

		private void ServiceHandler_PostProcessRequest(object sender, EventArgs e)
		{
			HttpContext context = (HttpContext)sender;
			// 2Checkout workaround for Direct Return = Yes
			context.Response.Clear();
			// write refresh html
			context.Response.Write("<html><head><META http-equiv=\"refresh\" content=\"0;" +
				"URL=" + GetProperty<string>(REDIRECT_URL) + "\"></head></html>");
		}
	}
}