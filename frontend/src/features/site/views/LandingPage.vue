<script setup lang="ts">
import { useRouter } from 'vue-router'
import ChatWidget from '@/features/site/components/ChatWidget.vue'
import { useLeroLero } from '@/features/site/composables/useLeroLero'

const router = useRouter()
const isLoggedIn = !!localStorage.getItem('token')
const lero = useLeroLero()

const featureBgs = ['bg-purple-500/20', 'bg-pink-500/20', 'bg-blue-500/20']

function goToApp() {
  router.push(isLoggedIn ? '/session/home' : '/login')
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900">
    <!-- Header -->
    <header class="fixed top-0 w-full bg-black/20 backdrop-blur-md z-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center py-4">
          <div class="flex items-center">
            <img src="/logo-sistema.png" alt="MeetConnect" class="h-9 w-auto object-contain" />
          </div>
          <button
            @click="goToApp"
            class="px-5 py-2 text-sm font-medium text-white bg-purple-600 hover:bg-purple-700 rounded-full transition"
          >
            {{ isLoggedIn ? 'Ir para o Painel' : 'Acessar Sistema' }}
          </button>
        </div>
      </div>
    </header>

    <!-- Hero Section -->
    <section class="pt-32 pb-20 px-4">
      <div class="max-w-7xl mx-auto text-center">
        <h1 class="text-5xl md:text-7xl font-bold text-white mb-6">
          {{ lero.heroTitulo1 }}
          <span class="bg-gradient-to-r from-purple-400 to-pink-400 bg-clip-text text-transparent">
            {{ lero.heroTitulo2 }}
          </span>
        </h1>
        <p class="text-xl text-gray-300 mb-10 max-w-2xl mx-auto">
          {{ lero.heroDesc }}
        </p>
        <div class="flex flex-col sm:flex-row gap-4 justify-center">
          <button
            @click="goToApp"
            class="px-8 py-4 text-lg font-semibold text-white bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 rounded-full transition shadow-lg shadow-purple-500/25"
          >
            {{ isLoggedIn ? 'Ir para o Painel' : lero.heroCtaPrimario }}
          </button>
          
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section id="features" class="py-20 px-4">
      <div class="max-w-7xl mx-auto">
        <h2 class="text-3xl md:text-4xl font-bold text-white text-center mb-16">
          {{ lero.featuresTitulo }}
        </h2>
        <div class="grid md:grid-cols-3 gap-8">
          <div
            v-for="(card, i) in lero.featureCards"
            :key="i"
            class="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10"
          >
            <div class="w-12 h-12 rounded-xl flex items-center justify-center mb-6" :class="featureBgs[i]">
              <span class="text-2xl">{{ card.emoji }}</span>
            </div>
            <h3 class="text-xl font-semibold text-white mb-3">{{ card.titulo }}</h3>
            <p class="text-gray-400">{{ card.descricao }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Pricing Section -->
    <section id="pricing" class="py-20 px-4">
      <div class="max-w-7xl mx-auto">
        <h2 class="text-3xl md:text-4xl font-bold text-white text-center mb-16">
          {{ lero.pricingTitulo }}
        </h2>
        <div class="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
          <!-- Plano Baixo -->
          <div class="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10">
            <h3 class="text-lg font-semibold text-gray-400 mb-2">{{ lero.planoBaixo.nome }}</h3>
            <div class="text-4xl font-bold text-white mb-6">R$ {{ lero.planoBaixo.preco }}<span class="text-lg text-gray-400">/mês</span></div>
            <ul class="space-y-3 text-gray-300">
              <li v-for="(item, j) in lero.planoBaixo.items" :key="j">{{ item }}</li>
            </ul>
          </div>
          <!-- Plano Popular -->
          <div class="bg-gradient-to-b from-purple-500/20 to-pink-500/20 backdrop-blur-sm rounded-2xl p-8 border border-purple-500/30 scale-105">
            <div class="text-sm font-semibold text-purple-400 mb-2">{{ lero.planoPop.badge }}</div>
            <h3 class="text-lg font-semibold text-gray-300 mb-2">{{ lero.planoPop.nome }}</h3>
            <div class="text-4xl font-bold text-white mb-6">R$ {{ lero.planoPop.preco }}<span class="text-lg text-gray-400">/mês</span></div>
            <ul class="space-y-3 text-gray-300">
              <li v-for="(item, j) in lero.planoPop.items" :key="j">{{ item }}</li>
            </ul>
          </div>
          <!-- Plano Alto -->
          <div class="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10">
            <h3 class="text-lg font-semibold text-gray-400 mb-2">{{ lero.planoAlto.nome }}</h3>
            <div class="text-4xl font-bold text-white mb-6">{{ lero.planoAlto.preco }}</div>
            <ul class="space-y-3 text-gray-300">
              <li v-for="(item, j) in lero.planoAlto.items" :key="j">{{ item }}</li>
            </ul>
          </div>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <footer id="about" class="py-12 px-4 border-t border-white/10">
      <div class="max-w-7xl mx-auto text-center">
        <div class="flex items-center justify-center gap-2 mb-4">
          <img src="/logo-sistema.png" alt="MeetConnect" class="h-7 w-auto object-contain" />
        </div>
        <p class="text-gray-400 text-sm">
          © 2026 MeetConnect. {{ lero.footerTexto }}
        </p>
      </div>
    </footer>

    <!-- Chat Widget -->
    <ChatWidget />

  </div>
</template>
