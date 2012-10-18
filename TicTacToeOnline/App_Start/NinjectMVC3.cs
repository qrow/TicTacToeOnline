using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Infrastructure;
using TicTacToeOnline.DbLogs;
using TicTacToeOnline.Engine;
using TicTacToeOnline.Hubs;
using TicTacToeOnline.Mailing;
using log4net;

[assembly: WebActivator.PreApplicationStartMethod(typeof(TicTacToeOnline.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(TicTacToeOnline.App_Start.NinjectMVC3), "Stop")]

namespace TicTacToeOnline.App_Start
{
    using System.Reflection;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Mvc;

    public static class NinjectMVC3 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
            DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));
            var kernel = CreateKernel();
            bootstrapper.Initialize(() => kernel);

            kernel.Get<GamesRoomGameEngineEventListner>(); //start listening game engine events
            kernel.Get<GameEngineListnerThatSendsEmails>(); //start listening game engine events
            kernel.Get<GameEngineListnerThatStoresLogsToDb>(); //start listening game engine events
            Database.SetInitializer(new CreateDatabaseIfNotExists<TicTacToeDbContext>());
            
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            AspNetHost.SetResolver(new SignalR.Ninject.NinjectDependencyResolver(kernel));
            kernel.Bind<IGameEngine>().To<GameEngine>().InSingletonScope();
            kernel.Bind(typeof(IRepository<>)).To(typeof(InMemoryRepository<>));
            kernel.Bind<Func<IConnectionManager>>().ToMethod(
                ctx => () => AspNetHost.DependencyResolver.Resolve<IConnectionManager>());

            kernel.Bind<Func<TicTacToeDbContext>>().ToMethod(ctx => () => ctx.Kernel.Get<TicTacToeDbContext>()).InSingletonScope();

            kernel.Bind<TicTacToeDbContext>()
                .ToSelf()
                .InSingletonScope()
                .WithConstructorArgument("nameOrConnectionString", "TicTacToeOnlineDb");
        }        
    }
}
