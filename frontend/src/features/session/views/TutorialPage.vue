<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth'
import { useToastStore } from '@/stores/toastStore'
import { sessionService } from '@/services/session.service'

const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()
const finishing = ref(false)
const imageExpanded = ref(false)

function openImage() {
  if (steps.value[currentStep.value].image) {
    imageExpanded.value = true
  }
}

function closeImage() {
  imageExpanded.value = false
}

// ── Steps ────────────────────────────────────────────────────────────────────
const currentStep = ref(0)

const steps = ref([
  {
    title: 'Bem-vindo ao MeetConnect',
    description: 'Este tutorial vai te guiar pelas funcionalidades do sistema de atendimento. Ao final, você terá acesso completo como operador.',
    image: '/logo-sistema.png',
    tip: 'Navegue usando os botões "Próximo" e "Anterior", ou clique diretamente nos indicadores.',
  },
  {
    title: 'Puxando uma Conversa',
    description: 'Na tela de atendimento, você verá todas as conversas disponíveis. Conversas aguardando aparecem com status "Aguardando" e você pode puxá-las para iniciar o atendimento.',
    image: '/tutorial/Tela de Atendimento - puxar conversa.jpeg',
    tip: 'Clique no botão "Puxar" para assumir uma conversa. Após puxar, você pode enviar mensagens normalmente.',
  },
  {
    title: 'Atendendo o Cliente',
    description: 'Após puxar a conversa, o chat fica ativo. Você pode enviar mensagens, ver o histórico e acompanhar o status em tempo real.',
    image: '/tutorial/Tela de Atendimento - atendimento.jpeg',
    tip: 'Use o campo de mensagem na parte inferior para responder ao cliente. As mensagens aparecem em tempo real.',
  },
  {
    title: 'Conferência com Operadores',
    description: 'Você pode convidar outros operadores para a mesma conversa. Isso permite atendimento em equipe quando o cliente precisa de suporte especializado.',
    image: '/tutorial/Tela de Atendimento - conferencia.jpeg',
    tip: 'Use o botão "Chamar" para convidar outro operador. Se você for o último atendente, o botão "Finalizar" encerrará a conversa.',
  },
  {
    title: 'Histórico de Conversas',
    description: 'Todas as conversas finalizadas ficam salvas no histórico. Você pode consultar conversas anteriores para referência.',
    image: '/tutorial/Tela de Atendimento - historico.jpeg',
    tip: 'O histórico é atualizado automaticamente quando uma conversa é finalizada.',
  },
  {
    title: 'Pronto para começar!',
    description: 'Agora que você conhece o básico, finalize o tutorial para liberar todas as funcionalidades do sistema. Você será promovido a Operador e terá acesso a todas as telas.',
    image: '/tutorial/Final do tutorial.png',
    tip: 'Clique em "Finalizar Tutorial" para prosseguir. Suas permissões serão atualizadas automaticamente.',
  },
])

const totalSteps = computed(() => steps.value.length)
const isFirstStep = computed(() => currentStep.value === 0)
const isLastStep = computed(() => currentStep.value === totalSteps.value - 1)
const progress = computed(() => ((currentStep.value + 1) / totalSteps.value) * 100)

function nextStep() {
  if (currentStep.value < totalSteps.value - 1) {
    currentStep.value++
  }
}

function prevStep() {
  if (currentStep.value > 0) {
    currentStep.value--
  }
}

function goToStep(index: number) {
  currentStep.value = index
}

async function finishTutorial() {
  finishing.value = true
  try {
    await sessionService.finishTutorial()
    toastStore.success('Tutorial concluído! Bem-vindo, Operador!')
    // Recarrega sessão para pegar novas permissões e views
    await authStore.checkAuth()
    router.push('/session/home')
  } catch {
    toastStore.error('Erro ao finalizar o tutorial. Tente novamente.')
  } finally {
    finishing.value = false
  }
}
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6">

    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Tutorial</h1>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          Passo {{ currentStep + 1 }} de {{ totalSteps }}
        </p>
      </div>
      <span class="text-xs font-medium text-purple-600 dark:text-purple-400 bg-purple-100 dark:bg-purple-900/30 px-3 py-1 rounded-full">
        Calouro
      </span>
    </div>

    <!-- Progress Bar -->
    <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
      <div
        class="bg-gradient-to-r from-purple-500 to-pink-500 h-2 rounded-full transition-all duration-500"
        :style="{ width: progress + '%' }"
      ></div>
    </div>

    <!-- Step Indicators -->
    <div class="flex items-center justify-center gap-2">
      <button
        v-for="(step, idx) in steps"
        :key="idx"
        @click="goToStep(idx)"
        :class="[
          'w-3 h-3 rounded-full transition-all duration-300',
          idx === currentStep
            ? 'bg-purple-600 scale-125'
            : idx < currentStep
              ? 'bg-purple-400'
              : 'bg-gray-300 dark:bg-gray-600'
        ]"
        :title="step.title"
      ></button>
    </div>

    <!-- Step Content Card -->
    <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">

      <!-- Image Area -->
      <div class="bg-gray-100 dark:bg-gray-900/50 flex items-center justify-center min-h-[320px] relative">
        <img
          v-if="steps[currentStep].image"
          :src="steps[currentStep].image!"
          :alt="steps[currentStep].title"
          class="max-w-full max-h-[400px] object-contain cursor-pointer hover:opacity-90 transition"
          @click="openImage"
        />
        <div v-else class="text-center text-gray-400 dark:text-gray-500 py-16">
          <i class="fas fa-image text-6xl mb-4 opacity-30"></i>
          <p class="text-sm">Imagem do passo será adicionada aqui</p>
          <p class="text-xs mt-1 text-gray-300 dark:text-gray-600">
            Coloque um screenshot da tela correspondente
          </p>
        </div>

        <!-- Step Number Badge -->
        <div class="absolute top-4 left-4 w-10 h-10 bg-purple-600 rounded-full flex items-center justify-center shadow-lg">
          <span class="text-white font-bold text-sm">{{ currentStep + 1 }}</span>
        </div>

        <!-- Expand hint -->
        <button
          v-if="steps[currentStep].image"
          @click="openImage"
          class="absolute top-4 right-4 w-8 h-8 bg-black/40 hover:bg-black/60 rounded-lg flex items-center justify-center transition"
          title="Ver em tela cheia"
        >
          <i class="fas fa-expand text-white text-xs"></i>
        </button>
      </div>

      <!-- Text Content -->
      <div class="p-8">
        <h2 class="text-xl font-bold text-gray-900 dark:text-white mb-3">
          {{ steps[currentStep].title }}
        </h2>
        <p class="text-gray-600 dark:text-gray-300 leading-relaxed mb-4">
          {{ steps[currentStep].description }}
        </p>

        <!-- Tip -->
        <div class="flex items-start gap-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800/40 rounded-xl px-4 py-3">
          <i class="fas fa-lightbulb text-amber-500 mt-0.5"></i>
          <p class="text-sm text-amber-800 dark:text-amber-200">
            {{ steps[currentStep].tip }}
          </p>
        </div>
      </div>
    </div>

    <!-- Navigation -->
    <div class="flex items-center justify-between">
      <button
        @click="prevStep"
        :disabled="isFirstStep"
        class="px-5 py-2.5 text-sm font-medium rounded-xl transition flex items-center gap-2 disabled:opacity-30 disabled:cursor-not-allowed text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600"
      >
        <i class="fas fa-arrow-left"></i>
        Anterior
      </button>

      <span class="text-xs text-gray-400">{{ currentStep + 1 }} / {{ totalSteps }}</span>

      <button
        v-if="!isLastStep"
        @click="nextStep"
        class="px-5 py-2.5 text-sm font-medium rounded-xl transition flex items-center gap-2 text-white bg-purple-600 hover:bg-purple-700"
      >
        Próximo
        <i class="fas fa-arrow-right"></i>
      </button>

      <button
        v-else
        @click="finishTutorial"
        :disabled="finishing"
        class="px-6 py-2.5 text-sm font-semibold rounded-xl transition flex items-center gap-2 text-white bg-gradient-to-r from-green-500 to-emerald-600 hover:from-green-600 hover:to-emerald-700 disabled:opacity-50 shadow-lg shadow-green-500/25"
      >
        <i :class="finishing ? 'fas fa-spinner fa-spin' : 'fas fa-check-circle'"></i>
        {{ finishing ? 'Finalizando...' : 'Finalizar Tutorial' }}
      </button>
    </div>

  </div>

  <!-- Fullscreen Modal -->
  <Teleport to="body">
    <Transition name="fade">
      <div
        v-if="imageExpanded"
        class="fixed inset-0 z-[100] bg-black/80 backdrop-blur-sm flex items-center justify-center p-6"
        @click.self="closeImage"
      >
        <div class="bg-gray-900 rounded-2xl overflow-hidden w-[95vw] h-[90vh] flex shadow-2xl">
          <!-- Image side -->
          <div class="flex-1 bg-black flex items-center justify-center p-6 min-w-0">
            <img
              :src="steps[currentStep].image!"
              :alt="steps[currentStep].title"
              class="w-full h-full object-contain"
            />
          </div>

          <!-- Text side -->
          <div class="w-80 flex-shrink-0 flex flex-col border-l border-gray-700">
            <!-- Header -->
            <div class="flex items-center justify-between p-4 border-b border-gray-700">
              <div class="flex items-center gap-2">
                <div class="w-8 h-8 bg-purple-600 rounded-full flex items-center justify-center">
                  <span class="text-white font-bold text-xs">{{ currentStep + 1 }}</span>
                </div>
                <span class="text-xs text-gray-400">Passo {{ currentStep + 1 }} de {{ totalSteps }}</span>
              </div>
              <button @click="closeImage" class="text-gray-400 hover:text-white transition">
                <i class="fas fa-times"></i>
              </button>
            </div>

            <!-- Content -->
            <div class="flex-1 overflow-y-auto p-5 space-y-4">
              <h3 class="text-lg font-bold text-white">
                {{ steps[currentStep].title }}
              </h3>
              <p class="text-sm text-gray-300 leading-relaxed">
                {{ steps[currentStep].description }}
              </p>

              <!-- Tip -->
              <div class="flex items-start gap-2 bg-amber-900/20 border border-amber-800/40 rounded-lg px-3 py-2">
                <i class="fas fa-lightbulb text-amber-500 mt-0.5 text-sm"></i>
                <p class="text-xs text-amber-200">
                  {{ steps[currentStep].tip }}
                </p>
              </div>
            </div>

            <!-- Navigation -->
            <div class="p-4 border-t border-gray-700 flex items-center justify-between">
              <button
                @click="prevStep"
                :disabled="isFirstStep"
                class="px-3 py-1.5 text-xs font-medium rounded-lg transition flex items-center gap-1 disabled:opacity-30 disabled:cursor-not-allowed text-gray-300 bg-gray-700 hover:bg-gray-600"
              >
                <i class="fas fa-arrow-left"></i>
                Anterior
              </button>
              <button
                v-if="!isLastStep"
                @click="nextStep"
                class="px-3 py-1.5 text-xs font-medium rounded-lg transition flex items-center gap-1 text-white bg-purple-600 hover:bg-purple-700"
              >
                Próximo
                <i class="fas fa-arrow-right"></i>
              </button>
              <button
                v-else
                @click="closeImage(); finishTutorial()"
                :disabled="finishing"
                class="px-3 py-1.5 text-xs font-semibold rounded-lg transition flex items-center gap-1 text-white bg-gradient-to-r from-green-500 to-emerald-600 hover:from-green-600 hover:to-emerald-700 disabled:opacity-50"
              >
                <i :class="finishing ? 'fas fa-spinner fa-spin' : 'fas fa-check-circle'"></i>
                {{ finishing ? 'Finalizando...' : 'Finalizar Tutorial' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
