'use client';

import { Database, Zap, Shield, Link as LinkIcon, Cloud } from 'lucide-react';

const integrations = [
  {
    icon: Database,
    name: 'External APIs',
    description: 'Full REST API support with webhooks and OAuth integration for seamless third-party connections.',
    features: ['REST API', 'Webhooks', 'OAuth', 'API Documentation'],
  },
  {
    icon: Cloud,
    name: 'Data Sources',
    description: 'Connect to databases, cloud storage, web services, and custom data providers.',
    features: ['Multiple Databases', 'Cloud Storage', 'Web Services', 'Streaming Data'],
  },
  {
    icon: LinkIcon,
    name: 'Third-party Tools',
    description: 'Seamless integration with Office suite, project management, CRM, and analytics platforms.',
    features: ['Office Integration', 'Project Management', 'CRM Systems', 'Analytics Tools'],
  },
  {
    icon: Shield,
    name: 'Security',
    description: 'Enterprise-grade security with encryption, access control, audit logging, and compliance.',
    features: ['Data Encryption', 'Access Control', 'Audit Logging', 'Compliance'],
  },
];

const codeExample = `// Example: Connect to External API
const httpClient = serviceProvider.GetRequiredService<HttpClient>();

// Authentication
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", apiToken);

// Make request
var response = await httpClient.GetAsync(
    "https://api.example.com/api/data"
);

var content = await response.Content.ReadAsAsync<ApiResponse>();`;

const geNuiExample = `// GenUI Card Integration Example
// Backend sends JSON envelope for structured responses

{"card":"stock","data":{
  "ticker":"AAPL",
  "company":"Apple Inc.",
  "price":189.30,
  "change":2.45,
  "changePct":1.31,
  "summary":"Apple trading near 52-week high"
}}`;

export default function Integration() {
  return (
    <section id="integration" className="py-20 md:py-32 bg-surface-primary">
      <div className="container-max">
        {/* Section Header */}
        <div className="max-w-3xl mx-auto text-center mb-16">
          <h2 className="section-heading">Integration Capabilities</h2>
          <p className="text-xl text-text-secondary">
            Powerful integration framework for connecting with external systems and data sources.
          </p>
        </div>

        {/* Integration Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-16">
          {integrations.map((integration) => {
            const IconComponent = integration.icon;
            return (
              <div key={integration.name} className="card hover:border-accent-primary/50 transition-all">
                <div className="flex items-start gap-4">
                  <div className="p-3 bg-accent-primary/20 rounded-lg">
                    <IconComponent size={24} className="text-accent-primary" />
                  </div>
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-text-primary mb-2">{integration.name}</h3>
                    <p className="text-text-secondary text-sm mb-4">{integration.description}</p>
                    <div className="flex flex-wrap gap-2">
                      {integration.features.map((feature) => (
                        <span
                          key={feature}
                          className="px-2 py-1 text-xs bg-surface-secondary text-text-secondary rounded"
                        >
                          {feature}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* Code Examples */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* HTTP Client Example */}
          <div>
            <h3 className="subsection-heading mb-4">HTTP Integration Example</h3>
            <p className="text-text-secondary mb-4 text-sm">
              Connect to external APIs with secure authentication and error handling:
            </p>
            <div className="code-block font-mono text-xs text-amber-50 overflow-x-auto">
              <pre>{codeExample}</pre>
            </div>
          </div>

          {/* GenUI Integration Example */}
          <div>
            <h3 className="subsection-heading mb-4">GenUI Card Response Format</h3>
            <p className="text-text-secondary mb-4 text-sm">
              Return structured data as JSON envelopes for rich card rendering in chat:
            </p>
            <div className="code-block font-mono text-xs text-amber-50 overflow-x-auto">
              <pre>{geNuiExample}</pre>
            </div>
          </div>
        </div>

        {/* Integration Types Grid */}
        <div className="mt-16">
          <h3 className="subsection-heading mb-8">Supported Integration Types</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {[
              'REST APIs',
              'WebSockets',
              'OAuth 2.0',
              'SQL Databases',
              'NoSQL Databases',
              'Cloud Storage',
              'Message Queues',
              'File Systems',
            ].map((type) => (
              <div
                key={type}
                className="p-4 bg-background rounded-lg border border-border-color text-center text-text-secondary hover:border-accent-primary/50 transition-colors"
              >
                {type}
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
