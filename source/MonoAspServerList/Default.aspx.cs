using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;

using Cheetah;
using Lidgren.Network;
using System.Threading;

namespace MonoAspServerList
{

	public partial class Default : System.Web.UI.Page
	{
        List<SpaceWar2006.GameSystem.GameServerInfo> infos = new List<SpaceWar2006.GameSystem.GameServerInfo>();

        protected void Answer(NetIncomingMessage msg)
        {
            SpaceWar2006.GameSystem.GameServerInfo info;
            {
                string p = msg.ReadString();
                info = new SpaceWar2006.GameSystem.GameServerInfo(p);
                infos.Add(info);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ServerFinder finder = new ServerFinder(new ServerFinder.AnswerDelegate(Answer), false, true);

            GridView1.AutoGenerateColumns = true;

            for (int i = 0; i < 5; ++i)
            {
                finder.Tick(0.5f);
                Thread.Sleep(500);
            }

            GridView1.DataSource = infos;
            GridView1.DataBind();
        }

		public virtual void button1Clicked (object sender, EventArgs args)
		{
			button1.Text = "You clicked me";
		}
	}
}

