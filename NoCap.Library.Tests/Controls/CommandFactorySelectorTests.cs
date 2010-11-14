using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Moq;
using NoCap.Library.Controls;
using NUnit.Framework;

namespace NoCap.Library.Tests.Controls {
    [TestFixture]
    class CommandFactorySelectorTests {
        [Test, RequiresSTA]
        public void SetCommandLeavesCommand() {
            var command = GetCommand();

            var cfs = new CommandFactorySelector {
                Command = command
            };

            DoEvents();

            Assert.AreEqual(command, cfs.Command);
            Assert.AreEqual(null, cfs.CommandFactory);
            Assert.AreEqual(null, cfs.InfoStuff);
        }
        
        [Test, RequiresSTA]
        public void SetCommandFactoryWithoutInfoStuffSets() {
            var commandFactory = GetCommandFactory();

            var cfs = new CommandFactorySelector {
                CommandFactory = commandFactory
            };

            DoEvents();

            Assert.AreEqual(null, cfs.Command);
            Assert.AreEqual(commandFactory, cfs.CommandFactory);
            Assert.AreEqual(null, cfs.InfoStuff);
        }
        
        [Test, RequiresSTA]
        public void SetCommandFactoryWithInfoStuffSets() {
            var commandFactory = GetCommandFactory();
            var infoStuff = GetInfoStuff(Enumerable.Empty<ICommand>(), new[] { commandFactory });

            var cfs = new CommandFactorySelector {
                CommandFactory = commandFactory,
                InfoStuff = infoStuff
            };

            DoEvents();

            Assert.AreEqual(commandFactory, cfs.CommandFactory);
            Assert.AreEqual(infoStuff, cfs.InfoStuff);
        }

        [Test, RequiresSTA]
        public void SetCommandFactoryWithBadInfoStuffSets() {
            var commandFactory = GetCommandFactory();
            var badCommandFactory = GetCommandFactory();
            var infoStuff = GetInfoStuff(Enumerable.Empty<ICommand>(), new[] { badCommandFactory });

            var cfs = new CommandFactorySelector {
                CommandFactory = commandFactory,
                InfoStuff = infoStuff
            };

            DoEvents();

            Assert.AreEqual(commandFactory, cfs.CommandFactory);
            Assert.AreEqual(infoStuff, cfs.InfoStuff);
        }

        [Test, RequiresSTA]
        public void SetInfoStuffShowsFactories() {
            var commandFactory1 = GetCommandFactory("1");
            var commandFactory2 = GetCommandFactory("2");
            var infoStuff = GetInfoStuff(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var cfs = new CommandFactorySelector {
                InfoStuff = infoStuff
            };

            DoEvents();

            var comboBox = GetComboBox(cfs);

            CollectionAssert.AreEquivalent(infoStuff.CommandFactories, comboBox.Items);
        }

        [Test, RequiresSTA]
        public void SetInfoStuffSelectsNull() {
            var commandFactory1 = GetCommandFactory("1");
            var commandFactory2 = GetCommandFactory("2");
            var infoStuff = GetInfoStuff(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var cfs = new CommandFactorySelector {
                InfoStuff = infoStuff
            };

            DoEvents();

            var comboBox = GetComboBox(cfs);

            Assert.AreEqual(null, comboBox.SelectedItem);
            Assert.AreEqual(null, cfs.CommandFactory);
        }

        [Test, RequiresSTA]
        public void SetCommandFactorySelects() {
            var commandFactory1 = GetCommandFactory("1");
            var commandFactory2 = GetCommandFactory("2");
            var infoStuff = GetInfoStuff(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var cfs = new CommandFactorySelector {
                InfoStuff = infoStuff,
                CommandFactory = commandFactory1
            };

            DoEvents();

            var comboBox = GetComboBox(cfs);

            Assert.AreEqual(commandFactory1, comboBox.SelectedItem);
            Assert.AreEqual(commandFactory1, cfs.CommandFactory);
        }

        [Test, RequiresSTA]
        public void SelectCommandFactorySetsCommand() {
            ICommand command = null;

            var commandFactory = GetCommandFactory(() => command);

            command = GetCommand(commandFactory);

            var infoStuff = GetInfoStuff(Enumerable.Empty<ICommand>(), new[] { commandFactory });

            var cfs = new CommandFactorySelector {
                InfoStuff = infoStuff,
                CommandFactory = commandFactory
            };

            DoEvents();

            Assert.AreEqual(command, cfs.Command);
        }

        [Test, RequiresSTA]
        public void SettingCommandLateSelectsFactory() {
            var commandFactory1 = GetCommandFactory("1");
            var commandFactory2 = GetCommandFactory("2");
            var infoStuff = GetInfoStuff(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var command = commandFactory2.CreateCommand();

            var cfs = new CommandFactorySelector {
                InfoStuff = infoStuff,
                Command = command
            };

            DoEvents();
            
            var comboBox = GetComboBox(cfs);

            Assert.AreEqual(commandFactory2, comboBox.SelectedItem);
            Assert.AreEqual(commandFactory2, cfs.CommandFactory);
        }

        [Test, RequiresSTA]
        public void SettingCommandEarlySelectsFactory() {
            var commandFactory1 = GetCommandFactory("1");
            var commandFactory2 = GetCommandFactory("2");
            var infoStuff = GetInfoStuff(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var command = commandFactory2.CreateCommand();

            var cfs = new CommandFactorySelector {
                Command = command,
                InfoStuff = infoStuff
            };

            DoEvents();
            
            var comboBox = GetComboBox(cfs);

            Assert.AreEqual(commandFactory2, comboBox.SelectedItem);
            Assert.AreEqual(commandFactory2, cfs.CommandFactory);
        }

        private static ComboBox GetComboBox(CommandFactorySelector selector) {
            return LogicalTreeHelper.GetChildren(selector).OfType<ComboBox>().First();
        }

        private static ICommand GetCommand() {
            ICommandFactory commandFactory; // Ignored

            return GetCommand(out commandFactory);
        }

        private static ICommand GetCommand(out ICommandFactory commandFactory) {
            ICommand command = null;

            commandFactory = GetCommandFactory(() => command);
            command = GetCommand(commandFactory);

            return command;
        }

        private static ICommand GetCommand(ICommandFactory factory) {
            var mockCommand = new Mock<ICommand>(MockBehavior.Strict);
            mockCommand.Setup((command) => command.GetFactory()).Returns(factory);

            return mockCommand.Object;
        }

        private static ICommandFactory GetCommandFactory(string name = "My factory") {
            ICommandFactory factory = null;
            factory = GetCommandFactory(() => GetCommand(factory), name);

            return factory;
        }

        private static ICommandFactory GetCommandFactory(Func<ICommand> commandGenerator, string name = "My factory") {
            var mockCommandFactory = new Mock<ICommandFactory>(MockBehavior.Strict);
            mockCommandFactory.Setup((factory) => factory.CreateCommand()).Returns(commandGenerator);
            mockCommandFactory.Setup((factory) => factory.Name).Returns(name);
            mockCommandFactory.Setup((factory) => factory.PopulateCommand(It.IsAny<ICommand>(), It.IsAny<IInfoStuff>()));

            return mockCommandFactory.Object;
        }

        private static IInfoStuff GetInfoStuff(IEnumerable<ICommand> commands, IEnumerable<ICommandFactory> commandFactories) {
            var commandsCollection = new ObservableCollection<ICommand>(commands);

            var mockInfoStuff = new Mock<IInfoStuff>(MockBehavior.Strict);
            mockInfoStuff.Setup((infoStuff) => infoStuff.CommandFactories).Returns(commandFactories);

            return mockInfoStuff.Object;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents() {
            var frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                frame.Continue = false;
            }));

            Dispatcher.PushFrame(frame);
        }
    }
}
