'use client';

import { Layers, Database, Shield, Zap } from 'lucide-react';

const layers = [
  {
    icon: Layers,
    name: 'Presentation Layer',
    path: 'Views/, Controls/, Resources/',
    description: 'XAML views, custom UI controls, and application themes with MVVM pattern implementation.',
    features: ['XAML Data Binding', 'Navigation System', 'Custom Controls', 'Theme Resources'],
  },
  {
    icon: Layers,
    name: 'View Model Layer',
    path: 'ViewModels/',
    description: 'Bridges UI and business logic with state management, command handling, and validation.',
    features: ['State Management', 'Command Handling', 'Data Transformation', 'Validation'],
  },
  {
    icon: Database,
    name: 'Business Logic Layer',
    path: 'Services/, Models/',
    description: 'Core business logic, service implementations, and domain entity definitions.',
    features: ['API Integration', 'Authentication', 'Session Management', 'Business Rules'],
  },
  {
    icon: Database,
    name: 'Data Access Layer',
    path: 'HTTP Client, Local Storage',
    description: 'Data persistence and external data source communication with caching.',
    features: ['REST API', 'Local Storage', 'File System', 'Data Caching'],
  },
];

const patterns = [
  {
    icon: Shield,
    title: 'Dependency Injection',
    description: 'Microsoft.Extensions.DependencyInjection for loose coupling and testability.',
  },
  {
    icon: Zap,
    title: 'MVVM Pattern',
    description: 'CommunityToolkit.Mvvm for clean separation of concerns and maintainability.',
  },
  {
    icon: Layers,
    title: 'Navigation Pattern',
    description: 'Type-safe navigation with centralized control and extensibility.',
  },
  {
    icon: Shield,
    title: 'Error Handling',
    description: 'Global exception handling with graceful recovery and proper logging.',
  },
];

export default function Architecture() {
  return (
    <section id="architecture" className="py-20 md:py-32 bg-surface-primary">
      <div className="container-max">
        {/* Section Header */}
        <div className="max-w-3xl mx-auto text-center mb-16">
          <h2 className="section-heading">Architecture</h2>
          <p className="text-xl text-text-secondary">
            Modern layered architecture built on WinUI 3 and .NET 8 with proven patterns.
          </p>
        </div>

        {/* Architecture Layers */}
        <div className="space-y-6 mb-16">
          {layers.map((layer, index) => {
            const IconComponent = layer.icon;
            return (
              <div key={layer.name} className="card">
                <div className="flex flex-col md:flex-row gap-6">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-3">
                      <div className="p-2 bg-accent-primary/20 rounded-lg">
                        <IconComponent size={20} className="text-accent-primary" />
                      </div>
                      <div>
                        <h3 className="text-xl font-semibold text-text-primary">{layer.name}</h3>
                        <p className="text-sm text-text-muted font-mono">{layer.path}</p>
                      </div>
                    </div>
                    <p className="text-text-secondary mb-4">{layer.description}</p>
                    <div className="flex flex-wrap gap-2">
                      {layer.features.map((feature) => (
                        <span
                          key={feature}
                          className="px-3 py-1 bg-surface-secondary rounded-full text-xs text-text-secondary"
                        >
                          {feature}
                        </span>
                      ))}
                    </div>
                  </div>
                  <div className="hidden md:block w-32 h-32 bg-gradient-to-br from-accent-primary/10 to-accent-secondary/10 rounded-lg flex-shrink-0"></div>
                </div>
              </div>
            );
          })}
        </div>

        {/* Core Patterns */}
        <div className="mt-20">
          <h3 className="subsection-heading mb-8">Core Architecture Patterns</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {patterns.map((pattern) => {
              const IconComponent = pattern.icon;
              return (
                <div key={pattern.title} className="bg-background rounded-lg p-6 border border-border-color">
                  <div className="flex items-start gap-4">
                    <div className="p-2 bg-accent-primary/20 rounded-lg flex-shrink-0">
                      <IconComponent size={20} className="text-accent-primary" />
                    </div>
                    <div>
                      <h4 className="font-semibold text-text-primary mb-2">{pattern.title}</h4>
                      <p className="text-text-secondary text-sm">{pattern.description}</p>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </section>
  );
}
