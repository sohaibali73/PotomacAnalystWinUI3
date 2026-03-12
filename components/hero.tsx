export default function Hero() {
  return (
    <section id="overview" className="pt-32 pb-20 md:pt-44 md:pb-28 px-6">
      <div className="max-w-[980px] mx-auto text-center">
        <p className="text-[#0071e3] text-[17px] md:text-[21px] font-semibold mb-3">
          Financial Analysis Toolkit
        </p>
        <h1 className="text-[40px] md:text-[64px] lg:text-[80px] font-semibold text-[#1d1d1f] leading-[1.05] tracking-[-0.015em] mb-6 text-balance">
          PotomacAnalyst
        </h1>
        <p className="text-[19px] md:text-[21px] text-[#86868b] leading-[1.381] max-w-[600px] mx-auto mb-10 text-pretty">
          A powerful WinUI 3 desktop application for advanced financial analysis, real-time data visualization, and AI-powered insights.
        </p>
        
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-20">
          <a
            href="#getting-started"
            className="inline-flex items-center justify-center px-7 py-3 bg-[#0071e3] text-white text-[17px] font-normal rounded-full hover:bg-[#0077ed] transition-colors min-w-[44px] min-h-[44px]"
          >
            Get Started
          </a>
          <a
            href="#api"
            className="inline-flex items-center gap-1 text-[#0071e3] text-[17px] font-normal hover:underline min-h-[44px]"
          >
            View API Reference
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </a>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 md:gap-0">
          <div className="md:border-r md:border-[#d2d2d7] px-8 py-4">
            <p className="text-[48px] md:text-[56px] font-semibold text-[#1d1d1f] leading-none mb-2">12+</p>
            <p className="text-[14px] text-[#86868b]">Core Modules</p>
          </div>
          <div className="md:border-r md:border-[#d2d2d7] px-8 py-4">
            <p className="text-[48px] md:text-[56px] font-semibold text-[#1d1d1f] leading-none mb-2">100+</p>
            <p className="text-[14px] text-[#86868b]">API Endpoints</p>
          </div>
          <div className="px-8 py-4">
            <p className="text-[48px] md:text-[56px] font-semibold text-[#1d1d1f] leading-none mb-2">AI</p>
            <p className="text-[14px] text-[#86868b]">Powered Insights</p>
          </div>
        </div>
      </div>
    </section>
  );
}
