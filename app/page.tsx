import Header from '@/components/header';
import Hero from '@/components/hero';
import GettingStarted from '@/components/getting-started';
import Modules from '@/components/modules';
import Architecture from '@/components/architecture';
import ApiReference from '@/components/api-reference';
import Integration from '@/components/integration';
import Footer from '@/components/footer';

// Server restart trigger
export default function Home() {
  return (
    <main className="min-h-screen bg-zinc-950">
      <Header />
      <Hero />
      <GettingStarted />
      <Modules />
      <Architecture />
      <ApiReference />
      <Integration />
      <Footer />
    </main>
  );
}
