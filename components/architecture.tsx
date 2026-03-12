const layers = [
  { 
    name: 'Presentation Layer', 
    path: 'Views/, Controls/, Resources/', 
    description: 'XAML views, custom UI controls, and application themes with MVVM pattern.',
    features: ['XAML Data Binding', 'Navigation System', 'Custom Controls', 'Theme Resources']
  },
  { 
    name: 'View Model Layer', 
    path: 'ViewModels/', 
    description: 'Bridges UI and business logic with state management and command handling.',
    features: ['State Management', 'Command Handling', 'Data Transformation', 'Validation']
  },
  { 
    name: 'Business Logic Layer', 
    path: 'Services/, Models/', 
    description: 'Core business logic, service implementations, and domain entity definitions.',
    features: ['API Integration', 'Authentication', 'Session Management', 'Business Rules']
  },
  { 
    name: 'Data Access Layer', 
    path: 'HTTP Client, Local Storage', 
    description: 'Data persistence and external data source communication with caching.',
    features: ['REST API', 'Local Storage', 'File System', 'Data Caching']
  },
];

const patterns = [
  { title: 'Dependency Injection', description: 'Microsoft.Extensions.DependencyInjection for loose coupling.' },
  { title: 'MVVM Pattern', description: 'CommunityToolkit.Mvvm for clean separation of concerns.' },
  { title: 'Navigation Pattern', description: 'Type-safe navigation with centralized control.' },
  { title: 'Error Handling', description: 'Global exception handling with graceful recovery.' },
];

export default function Architecture() {
  return (
    <section id="architecture" className="py-20 md:py-28 bg-[#f5f5f7]">
      <div className="max-w-[980px] mx-auto px-6">
        <div className="text-center mb-16">
          <h2 className="text-[32px] md:text-[48px] font-semibold text-[#1d1d1f] leading-[1.08] tracking-[-0.003em] mb-4">
            Modern architecture.
          </h2>
          <p className="text-[19px] md:text-[21px] text-[#86868b] max-w-[600px] mx-auto">
            Built on WinUI 3 and .NET 8 with proven design patterns.
          </p>
        </div>

        <div className="space-y-4 mb-16">
          {layers.map((layer, index) => (
            <div key={layer.name} className="bg-white rounded-2xl p-6 md:p-8 shadow-sm">
              <div className="flex flex-col md:flex-row md:items-start gap-6">
                <div className="w-12 h-12 bg-[#0071e3] text-white rounded-full flex items-center justify-center text-[17px] font-semibold flex-shrink-0">
                  {index + 1}
                </div>
                <div className="flex-1">
                  <div className="mb-3">
                    <h3 className="text-[21px] font-semibold text-[#1d1d1f]">{layer.name}</h3>
                    <p className="text-[12px] font-mono text-[#86868b] mt-1">{layer.path}</p>
                  </div>
                  <p className="text-[17px] text-[#86868b] mb-4">{layer.description}</p>
                  <div className="flex flex-wrap gap-2">
                    {layer.features.map((feature) => (
                      <span 
                        key={feature} 
                        className="px-3 py-1.5 bg-[#f5f5f7] rounded-full text-[12px] text-[#1d1d1f]"
                      >
                        {feature}
                      </span>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>

        <h3 className="text-[24px] font-semibold text-[#1d1d1f] text-center mb-8">Core Patterns</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {patterns.map((pattern) => (
            <div key={pattern.title} className="bg-white rounded-2xl p-6 shadow-sm">
              <h4 className="text-[17px] font-semibold text-[#1d1d1f] mb-2">{pattern.title}</h4>
              <p className="text-[14px] text-[#86868b]">{pattern.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
