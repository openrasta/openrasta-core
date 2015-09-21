using System;

namespace WindsorDependencyResolver_Specification
{
    class DependencyOnTypeWithGernericParams
    {
        public String Dependency { get; private set; }
        
        public DependencyOnTypeWithGernericParams(String dependency)
        {
            Dependency = dependency;
        }
    }
}