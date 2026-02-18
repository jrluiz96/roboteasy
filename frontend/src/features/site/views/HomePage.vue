<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

function goToLogin() {
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900">
    <!-- Header -->
    <header class="fixed top-0 w-full bg-black/20 backdrop-blur-md z-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center py-4">
          <div class="flex items-center gap-2">
            <div class="w-8 h-8 bg-gradient-to-br from-purple-500 to-pink-500 rounded-lg"></div>
            <span class="text-xl font-bold text-white">RobotEasy</span>
          </div>
          <nav class="hidden md:flex items-center gap-8">
            <a href="#features" class="text-gray-300 hover:text-white transition">Recursos</a>
            <a href="#pricing" class="text-gray-300 hover:text-white transition">Preços</a>
            <a href="#about" class="text-gray-300 hover:text-white transition">Sobre</a>
          </nav>
          <button
            @click="goToLogin"
            class="px-5 py-2 text-sm font-medium text-white bg-purple-600 hover:bg-purple-700 rounded-full transition"
          >
            Acessar Sistema
          </button>
        </div>
      </div>
    </header>

    <!-- Hero Section -->
    <section class="pt-32 pb-20 px-4">
      <div class="max-w-7xl mx-auto text-center">
        <h1 class="text-5xl md:text-7xl font-bold text-white mb-6">
          Automatize seus
          <span class="bg-gradient-to-r from-purple-400 to-pink-400 bg-clip-text text-transparent">
            processos
          </span>
        </h1>
        <p class="text-xl text-gray-300 mb-10 max-w-2xl mx-auto">
          Plataforma completa para gestão e automação de robôs. 
          Simplifique operações, aumente produtividade e escale seu negócio.
        </p>
        <div class="flex flex-col sm:flex-row gap-4 justify-center">
          <button
            @click="goToLogin"
            class="px-8 py-4 text-lg font-semibold text-white bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 rounded-full transition shadow-lg shadow-purple-500/25"
          >
            Começar Agora
          </button>
          <a
            href="#features"
            class="px-8 py-4 text-lg font-semibold text-white border border-white/20 hover:bg-white/10 rounded-full transition"
          >
            Saiba Mais
          </a>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section id="features" class="py-20 px-4">
      <div class="max-w-7xl mx-auto">
        <h2 class="text-3xl md:text-4xl font-bold text-white text-center mb-16">
          Tudo que você precisa
        </h2>
        <div class="grid md:grid-cols-3 gap-8">
          <div class="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10">
            <div class="w-12 h-12 bg-purple-500/20 rounded-xl flex items-center justify-center mb-6">
              <span class="text-2xl">🤖</span>
            </div>
            <h3 class="text-xl font-semibold text-white mb-3">Gestão de Robôs</h3>
            <p class="text-gray-400">
              Controle todos os seus robôs em um único painel. Monitore status, logs e métricas em tempo real.
            </p>
          </div>
          <div class="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10">
            <div class="w-12 h-12 bg-pink-500/20 rounded-xl flex items-center justify-center mb-6">
              <span class="text-2xl">📊</span>
            </div>
            <h3 class="text-xl font-semibold text-white mb-3">Dashboards</h3>
            <p class="text-gray-400">
              Visualize dados e métricas importantes com dashboards personalizáveis e relatórios detalhados.
            </p>
          </div>
          <div class="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10">
            <div class="w-12 h-12 bg-blue-500/20 rounded-xl flex items-center justify-center mb-6">
              <span class="text-2xl">🔒</span>
            </div>
            <h3 class="text-xl font-semibold text-white mb-3">Segurança</h3>
            <p class="text-gray-400">
              Autenticação robusta, permissões granulares e logs de auditoria para máxima segurança.
            </p>
          </div>
        </div>
      </div>
    </section>

    <!-- Pricing Section -->
    <section id="pricing" class="py-20 px-4">
      <div class="max-w-7xl mx-auto">
        <h2 class="text-3xl md:text-4xl font-bold text-white text-center mb-16">
          Planos flexíveis
        </h2>
        <div class="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
          <div class="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10">
            <h3 class="text-lg font-semibold text-gray-400 mb-2">Starter</h3>
            <div class="text-4xl font-bold text-white mb-6">R$ 99<span class="text-lg text-gray-400">/mês</span></div>
            <ul class="space-y-3 text-gray-300 mb-8">
              <li>✓ Até 5 robôs</li>
              <li>✓ Dashboard básico</li>
              <li>✓ Suporte por email</li>
            </ul>
            <button class="w-full py-3 rounded-full border border-white/20 text-white hover:bg-white/10 transition">
              Escolher
            </button>
          </div>
          <div class="bg-gradient-to-b from-purple-500/20 to-pink-500/20 backdrop-blur-sm rounded-2xl p-8 border border-purple-500/30 scale-105">
            <div class="text-sm font-semibold text-purple-400 mb-2">POPULAR</div>
            <h3 class="text-lg font-semibold text-gray-300 mb-2">Pro</h3>
            <div class="text-4xl font-bold text-white mb-6">R$ 299<span class="text-lg text-gray-400">/mês</span></div>
            <ul class="space-y-3 text-gray-300 mb-8">
              <li>✓ Até 25 robôs</li>
              <li>✓ Dashboards avançados</li>
              <li>✓ API completa</li>
              <li>✓ Suporte prioritário</li>
            </ul>
            <button class="w-full py-3 rounded-full bg-gradient-to-r from-purple-600 to-pink-600 text-white hover:from-purple-700 hover:to-pink-700 transition">
              Escolher
            </button>
          </div>
          <div class="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10">
            <h3 class="text-lg font-semibold text-gray-400 mb-2">Enterprise</h3>
            <div class="text-4xl font-bold text-white mb-6">Custom</div>
            <ul class="space-y-3 text-gray-300 mb-8">
              <li>✓ Robôs ilimitados</li>
              <li>✓ Deploy on-premise</li>
              <li>✓ SLA garantido</li>
              <li>✓ Suporte dedicado</li>
            </ul>
            <button class="w-full py-3 rounded-full border border-white/20 text-white hover:bg-white/10 transition">
              Contato
            </button>
          </div>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <footer id="about" class="py-12 px-4 border-t border-white/10">
      <div class="max-w-7xl mx-auto text-center">
        <div class="flex items-center justify-center gap-2 mb-4">
          <div class="w-6 h-6 bg-gradient-to-br from-purple-500 to-pink-500 rounded-lg"></div>
          <span class="text-lg font-bold text-white">RobotEasy</span>
        </div>
        <p class="text-gray-400 text-sm">
          © 2026 RobotEasy. Todos os direitos reservados.
        </p>
      </div>
    </footer>
  </div>
</template>
