const integrations = [
  { 
    name: 'External APIs', 
    description: 'Full REST API support with webhooks and OAuth integration.',
    features: ['REST API', 'Webhooks', 'OAuth', 'Documentation']
  },
  { 
    name: 'Data Sources', 
    description: 'Connect to databases, cloud storage, and custom data providers.',
    features: ['Databases', 'Cloud Storage', 'Web Services', 'Streaming']
  },
  { 
    name: 'Third-party Tools', 
    description: 'Seamless integration with Office, CRM, and analytics platforms.',
    features: ['Office Suite', 'Project Management', 'CRM Systems', 'Analytics']
  },
  { 
    name: 'Security', 
    description: 'Enterprise-grade security with encryption and access control.',
    features: ['Encryption', 'Access Control', 'Audit Logging', 'Compliance']
  },
];

const codeExample = `// Connect to External API
var httpClient = serviceProvider.GetRequiredService<HttpClient>();

httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", apiToken);

var response = await httpClient.GetAsync(
    "https://api.example.com/data"
);`;

export default function Integration() {
  return (
    <section id="integration" className="py-20 md:py-28 bg-[#f5f5f7]">
      <div className="max-w-[980px] mx-auto px-6">
        <div className="text-center mb-16">
          <h2 className="text-[32px] md:text-[48px] font-semibold text-[#1d1d1f] leading-[1.08] tracking-[-0.003em] mb-4">
            Seamless integration.
          </h2>
          <p className="text-[19px] md:text-[21px] text-[#86868b] max-w-[600px] mx-auto">
            Connect with external systems and data sources effortlessly.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-5 mb-12">
          {integrations.map((integration) => (
            <div key={integration.name} className="bg-white rounded-2xl p-6 md:p-8 shadow-sm">
              <h3 className="text-[21px] font-semibold text-[#1d1d1f] mb-2">{integration.name}</h3>
              <p className="text-[14px] text-[#86868b] mb-4">{integration.description}</p>
              <div className="flex flex-wrap gap-2">
                {integration.features.map((feature) => (
                  <span 
                    key={feature} 
                    className="px-3 py-1.5 bg-[#f5f5f7] rounded-full text-[12px] text-[#1d1d1f]"
                  >
                    {feature}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>

        <div className="bg-white rounded-2xl p-6 md:p-8 shadow-sm">
          <h3 className="text-[21px] font-semibold text-[#1d1d1f] mb-4">Integration Example</h3>
          <p className="text-[14px] text-[#86868b] mb-6">Connect to external APIs with secure authentication:</p>
          <div className="bg-[#1d1d1f] rounded-xl p-5 overflow-x-auto">
            <pre className="text-[14px] font-mono text-[#f5f5f7] leading-relaxed">
              <code>{codeExample}</code>
            </pre>
          </div>
        </div>

        <div className="mt-8 grid grid-cols-2 md:grid-cols-4 gap-4">
          {['REST APIs', 'WebSockets', 'OAuth 2.0', 'SQL Databases', 'NoSQL', 'Cloud Storage', 'Message Queues', 'File Systems'].map((type) => (
            <div key={type} className="bg-white rounded-xl p-4 text-center shadow-sm">
              <span className="text-[14px] text-[#1d1d1f]">{type}</span>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
