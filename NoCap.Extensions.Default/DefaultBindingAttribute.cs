using System;
using WinputDotNet;

namespace NoCap.Extensions.Default {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DefaultBindingAttribute : Attribute {
        private readonly Type providerType;
        private readonly IInputSequence inputSequence;

        public DefaultBindingAttribute(Type providerType, Type inputSequenceType, params object[] constructorParameters) {
            this.providerType = providerType;
            this.inputSequence = (IInputSequence) Activator.CreateInstance(inputSequenceType, constructorParameters);
        }

        public DefaultBindingAttribute(Type providerType, IInputSequence inputSequence) {
            this.providerType = providerType;
            this.inputSequence = inputSequence;
        }

        public Type ProviderType {
            get { return this.providerType; }
        }

        public IInputSequence InputSequence {
            get { return this.inputSequence; }
        }
    }
}
