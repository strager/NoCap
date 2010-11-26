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
            Assert.AreEqual(null, cfs.CommandProvider);
        }
        
        [Test, RequiresSTA]
        public void SetCommandFactoryWithoutCommandProviderSets() {
            var commandFactory = GetCommandFactory();

            var cfs = new CommandFactorySelector {
                CommandFactory = commandFactory
            };

            DoEvents();

            Assert.AreEqual(null, cfs.Command);
            Assert.AreEqual(commandFactory, cfs.CommandFactory);
            Assert.AreEqual(null, cfs.CommandProvider);
        }
        
        [Test, RequiresSTA]
        public void SetCommandFactoryWithCommandProviderSets() {
            var commandFactory = GetCommandFactory();
            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { commandFactory });

            var cfs = new CommandFactorySelector {
                CommandFactory = commandFactory,
                CommandProvider = commandProvider
            };

            DoEvents();

            Assert.AreEqual(commandFactory, cfs.CommandFactory);
            Assert.AreEqual(commandProvider, cfs.CommandProvider);
        }

        [Test, RequiresSTA]
        public void SetCommandFactoryWithBadCommandProviderSets() {
            var commandFactory = GetCommandFactory();
            var badCommandFactory = GetCommandFactory();
            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { badCommandFactory });

            var cfs = new CommandFactorySelector {
                CommandFactory = commandFactory,
                CommandProvider = commandProvider
            };

            DoEvents();

            Assert.AreEqual(commandFactory, cfs.CommandFactory);
            Assert.AreEqual(commandProvider, cfs.CommandProvider);
        }

        [Test, RequiresSTA]
        public void SetCommandProviderShowsFactories() {
            var commandFactory1 = GetCommandFactory("1");
            var commandFactory2 = GetCommandFactory("2");
            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var cfs = new CommandFactorySelector {
                CommandProvider = commandProvider
            };

            DoEvents();

            var comboBox = GetComboBox(cfs);

            CollectionAssert.AreEquivalent(commandProvider.CommandFactories, comboBox.Items);
        }

        [Test, RequiresSTA]
        public void SetCommandProviderSelectsNull() {
            var commandFactory1 = GetCommandFactory("1");
            var commandFactory2 = GetCommandFactory("2");
            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var cfs = new CommandFactorySelector {
                CommandProvider = commandProvider
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
            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var cfs = new CommandFactorySelector {
                CommandProvider = commandProvider,
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

            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { commandFactory });

            var cfs = new CommandFactorySelector {
                CommandProvider = commandProvider,
                CommandFactory = commandFactory
            };

            DoEvents();

            Assert.AreEqual(command, cfs.Command);
        }

        [Test, RequiresSTA]
        public void SettingCommandLateSelectsFactory() {
            var commandFactory1 = GetCommandFactory("1");
            var commandFactory2 = GetCommandFactory("2");
            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var command = commandFactory2.CreateCommand();

            var cfs = new CommandFactorySelector {
                CommandProvider = commandProvider,
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
            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { commandFactory1, commandFactory2 });

            var command = commandFactory2.CreateCommand();

            var cfs = new CommandFactorySelector {
                Command = command,
                CommandProvider = commandProvider
            };

            DoEvents();
            
            var comboBox = GetComboBox(cfs);

            Assert.AreEqual(commandFactory2, comboBox.SelectedItem);
            Assert.AreEqual(commandFactory2, cfs.CommandFactory);
        }

        [Test, RequiresSTA]
        public void SettingFilterDoesNotChangeCommand() {
            var fileCommandFactory = GetCommandFactory("file", CommandFeatures.FileUploader);
            var textCommandFactory = GetCommandFactory("text", CommandFeatures.TextUploader);
            var commandProvider = GetCommandProvider(Enumerable.Empty<ICommand>(), new[] { fileCommandFactory, textCommandFactory });

            var command = fileCommandFactory.CreateCommand();

            var cfs = new CommandFactorySelector {
                Command = command,
                CommandProvider = commandProvider
            };

            DoEvents();

            cfs.Filter = CommandFeatures.TextUploader;

            DoEvents();

            Assert.AreEqual(command, cfs.Command);
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

        private static ICommandFactory GetCommandFactory(string name = "My factory", CommandFeatures features = (CommandFeatures) 0) {
            ICommandFactory factory = null;
            factory = GetCommandFactory(() => GetCommand(factory), name, features);

            return factory;
        }

        private static ICommandFactory GetCommandFactory(Func<ICommand> commandGenerator, string name = "My factory", CommandFeatures features = (CommandFeatures) 0) {
            var mockCommandFactory = new Mock<ICommandFactory>(MockBehavior.Strict);
            mockCommandFactory.Setup((factory) => factory.CreateCommand()).Returns(commandGenerator);
            mockCommandFactory.Setup((factory) => factory.Name).Returns(name);
            mockCommandFactory.Setup((factory) => factory.CommandFeatures).Returns(features);
            mockCommandFactory.Setup((factory) => factory.PopulateCommand(It.IsAny<ICommand>(), It.IsAny<ICommandProvider>()));

            return mockCommandFactory.Object;
        }

        private static ICommandProvider GetCommandProvider(IEnumerable<ICommand> commands, IEnumerable<ICommandFactory> commandFactories) {
            var commandsCollection = new ObservableCollection<ICommand>(commands);

            var mockCommandProvider = new Mock<ICommandProvider>(MockBehavior.Strict);
            mockCommandProvider.Setup((commandProvider) => commandProvider.CommandFactories).Returns(commandFactories);

            return mockCommandProvider.Object;
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
