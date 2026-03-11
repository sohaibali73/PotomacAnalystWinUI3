'use client';

import { ArrowRight, BookOpen, Zap } from 'lucide-react';

export default function Hero() {
  return (
    <section className="relative overflow-hidden bg-gradient-to-b from-zinc-900 via-zinc-950 to-zinc-950 pt-20 md:pt-32 pb-16 md:pb-24">
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-0 right-0 w-96 h-96 bg-blue-500/5 rounded-full blur-3xl"></div>
        <div className="absolute bottom-0 left-0 w-96 h-96 bg-yellow-400/5 rounded-full blur-3xl"></div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
        <div className="max-w-3xl mx-auto text-center">
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-zinc-800 rounded-full border border-zinc-700 mb-6">
            <Zap size={16} className="text-yellow-400" />
            <span className="text-sm text-zinc-400">Complete API Documentation</span>
          </div>

          <h1 className="text-4xl md:text-6xl font-bold text-zinc-100 mb-6 leading-tight text-balance">
            Build with{' '}
            <span className="bg-gradient-to-r from-yellow-400 via-amber-300 to-yellow-200 bg-clip-text text-transparent">
              PotomacAnalyst
            </span>
          </h1>

          <p className="text-lg md:text-xl text-zinc-400 mb-8 max-w-2xl mx-auto text-pretty">
            Advanced financial analysis toolkit built on modern architecture. Explore comprehensive documentation, integration guides, and API references to build powerful analytical applications.
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center mb-16">
            <a 
              href="#getting-started" 
              className="px-6 py-3 bg-yellow-400 text-zinc-900 font-semibold rounded-lg hover:bg-yellow-300 transition-colors inline-flex items-center justify-center gap-2 min-h-[44px]"
            >
              Get Started
              <ArrowRight size={18} />
            </a>
            <a 
              href="#modules" 
              className="px-6 py-3 bg-zinc-800 text-zinc-100 font-semibold rounded-lg border border-zinc-700 hover:bg-zinc-700 transition-colors inline-flex items-center justify-center gap-2 min-h-[44px]"
            >
              <BookOpen size={18} />
              Explore Modules
            </a>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-12 border-t border-zinc-800">
            <div className="text-center">
              <div className="text-3xl font-bold text-yellow-400 mb-2">13+</div>
              <p className="text-zinc-400">Core Modules</p>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-yellow-400 mb-2">.NET 8</div>
              <p className="text-zinc-400">Modern Stack</p>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-yellow-400 mb-2">100%</div>
              <p className="text-zinc-400">WinUI 3</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
