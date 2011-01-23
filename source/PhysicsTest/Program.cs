﻿using System;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Collections.Generic;

using Cheetah;
using Cheetah.Graphics;

using OpenTK;

using Cheetah.Physics;

namespace PhysicsTest
{

    class Car : PhysicsNode
    {
        public Car()
        {
            Draw = new System.Collections.ArrayList();
            Draw.Add(new Marker());
        }

        protected override IPhysicsObject CreatePhysicsObject(Scene s)
        {
            return s.Physics.CreateObjectCar();
        }
    }

    class Cube : PhysicsNode
    {
        public Cube()
        {
            Draw = new System.Collections.ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("cube/cube.mesh"));
        }

        protected override IPhysicsObject CreatePhysicsObject(Scene s)
        {
            return s.Physics.CreateObjectBox(1, 2, 2, 2);
        }
    }

    class Floor : PhysicsNode
    {
        public Floor()
        {
            Draw = new System.Collections.ArrayList();
            Draw.Add(Root.Instance.ResourceManager.LoadMesh("floor/floor.mesh"));
        }

        protected override IPhysicsObject CreatePhysicsObject(Scene s)
        {
            IPhysicsObject obj=s.Physics.CreateObjectBox(1, 200, 2, 200);
            obj.Movable = false;
            return obj;
        }
    }

    class PhysicsServer : Flow
    {
        public override void Start()
        {
            base.Start();

            for (int i = 0; i < 20; ++i)
            {
                Cube c = new Cube();
                Root.Instance.Scene.Spawn(c);
                c.Position = new Vector3(0, 10 + i * 2, 0);
                cubes.Add(c);
            }
            Root.Instance.Scene.Spawn(new Floor());

            Root.Instance.Scene.Spawn(light = new Light());
            light.Position = new Vector3(1, 1, 1);
            light.directional = true;
            light.diffuse = new Color4f(0.5f, 0.5f, 0.5f);
            Root.Instance.Scene.Spawn(light = new Light());
            light.Position = new Vector3(-20, 20, 20);
            light.diffuse = new Color4f(0.6f, 0, 0);
            Root.Instance.Scene.Spawn(light = new Light());
            light.Position = new Vector3(20, 20, -20);
            light.diffuse = new Color4f(0, 0.6f, 0);
            Root.Instance.Scene.Spawn(light = new Light());
            light.Position = new Vector3(-20, 20, -20);
            light.diffuse = new Color4f(0, 0, 0.6f);
        }
        List<Cube> cubes = new List<Cube>();
        Light light;
    }

    class PhysicsClient : Flow
    {
        PhysicsServer server;

        public PhysicsClient(bool local)
        {
            if (local)
            {
                server = new PhysicsServer();
            }
        }

        public override void Start()
        {
            base.Start();

            if (server != null)
                server.Start();

            camera = new Camera();
            camera.Position = new Vector3(40, 40, 40);
            camera.LookAt(0, 5, 0);
            Root.Instance.LocalObjects.Add(camera);

            Root.Instance.Scene.camera = camera;

            x = Root.Instance.UserInterface.Mouse.GetPosition(0);
            y = Root.Instance.UserInterface.Mouse.GetPosition(1);
        }

        float x;
        float y;
        float ax;
        float ay;

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (server != null)
                server.Tick(dtime);

            //camera.Position = new Vector3((float)Math.Cos(Root.Instance.Time)*40, camera.Position.Y, (float)Math.Sin(Root.Instance.Time)*40);
            //camera.LookAt(cubes[cubes.Count -20].Position);

            /*if (Time > 10)
            {
                Start();
                Time = 0;
            }*/

            float tx=Root.Instance.UserInterface.Mouse.GetPosition(0);
            float ty=Root.Instance.UserInterface.Mouse.GetPosition(1);

            float dx = tx - x;
            float dy = ty - y;

            System.Console.WriteLine(dx);
            System.Console.WriteLine(dy);

            if (Root.Instance.UserInterface.Mouse.GetButtonState(1))
            {
                ax += dx;
                ay += dy;
            }

            if (Root.Instance.UserInterface.Keyboard.GetButtonState((int)global::OpenTK.Input.Key.W))
            {
                camera.Speed = camera.Direction * 50;
            }
            else if (Root.Instance.UserInterface.Keyboard.GetButtonState((int)global::OpenTK.Input.Key.S))
            {
                camera.Speed = camera.Direction * -50;
            }
            else
            {
                camera.Speed = Vector3.Zero;
            }

            camera.Orientation = Quaternion.FromAxisAngle(Vector3.UnitY, ax * -0.005f) * Quaternion.FromAxisAngle(Vector3.UnitX, ay * 0.005f);
            //camera.rotationspeed.Y = dy;

            x = tx;
            y = ty;
        }

        public override void OnKeyPress(OpenTK.Input.Key k)
        {
            base.OnKeyPress(k);

            if (k == OpenTK.Input.Key.Space)
            {
                Root.Instance.UserInterface.CaptureMouse = true;
                Cube c = new Cube();
                Root.Instance.Scene.Spawn(c);
                c.Position = camera.Position;
                c.Orientation = camera.Orientation;
                c.Physics.Speed = camera.Direction*100;
                //cubes.Add(c);

            }
            else if (k == OpenTK.Input.Key.C)
            {
                Car c = new Car();
                Root.Instance.Scene.Spawn(c);
                c.Position = camera.Position;
                c.Orientation = camera.Orientation;
                c.Physics.Speed = camera.Direction * 10;

            }
        }
        Camera camera;
    }

    class Program
    {
        static void ServerMain(string[] args)
        {
            Root r = new Root(args, true);
            r.ServerServer(args);

            r.NextIndex += 10;

            while (!Root.Instance.Quit)
            {
                Flow f = new PhysicsServer();
                r.CurrentFlow = f;
                f.Start();
                r.ServerRun(true);
                f.Stop();
            }

            r.ServerStop();
            r.Dispose();
        }

        public static void ClientMain(string[] args)
        {
            Root r = new Root(args, false);
            r.ClientClient(args);
            IUserInterface ui = r.UserInterface;

            Flow f = new PhysicsClient(true);

            r.CurrentFlow = f;

            f.Start();

            r.ClientLoop();

            r.Dispose();
        }

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");

            string dir = Directory.GetCurrentDirectory();
            Assembly a = Assembly.GetEntryAssembly();
            System.Console.WriteLine("assembly path:" + a.Location);

            int i = Array.IndexOf<string>(args, "-root");
            if (i != -1)
            {
                string rootdir = args[i + 1];
                Directory.SetCurrentDirectory(rootdir);
                System.Console.WriteLine("root directory: " + rootdir);
            }
            else
            {
                DirectoryInfo current = new FileInfo(a.Location).Directory;
                while (current.GetFiles("cheetah_root").Length == 0)
                {
                    if ((current = current.Parent) == null)
                    {
                        throw new Exception("Can't find game root directory. Use -root $directory.");
                    }
                }
                Directory.SetCurrentDirectory(current.FullName);
                System.Console.WriteLine("root directory: " + current.FullName);
            }


            if (Array.IndexOf<string>(args, "server") != -1)
            {
                ServerMain(args);
            }
            else if (Array.IndexOf<string>(args, "client") != -1)
            {
                ClientMain(args);
            }
            /*else if (Array.IndexOf<string>(args, "clientserver") != -1)
            {
                System.Console.WriteLine("client started. launching server...");
                Process server = Process.Start("Game.exe", "server");
                Thread.Sleep(1000);
                System.Console.WriteLine("done.");

                ClientMain(args);

                server.Kill();
            }*/
            //ServerMain(args);
            //ClientMain(args);
        }
    }
}