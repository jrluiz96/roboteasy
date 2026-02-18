<script setup lang="ts">
import { useToastStore, type ToastType } from '@/stores/toastStore'

const toastStore = useToastStore()

function getToastClasses(type: ToastType): string {
  const base = 'flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg text-sm font-medium transition-all duration-300'
  
  const variants: Record<ToastType, string> = {
    success: 'bg-green-600 text-white',
    error: 'bg-red-600 text-white',
    warning: 'bg-yellow-500 text-black',
    info: 'bg-blue-600 text-white'
  }
  
  return `${base} ${variants[type]}`
}

function getIcon(type: ToastType): string {
  const icons: Record<ToastType, string> = {
    success: '✓',
    error: '✕',
    warning: '⚠',
    info: 'ℹ'
  }
  return icons[type]
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed top-4 right-4 z-50 flex flex-col gap-2 max-w-sm">
      <TransitionGroup name="toast">
        <div
          v-for="toast in toastStore.toasts"
          :key="toast.id"
          :class="getToastClasses(toast.type)"
        >
          <span class="text-lg">{{ getIcon(toast.type) }}</span>
          <span class="flex-1">{{ toast.message }}</span>
          <button 
            @click="toastStore.remove(toast.id)"
            class="opacity-70 hover:opacity-100 text-lg leading-none"
          >
            ×
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.toast-move {
  transition: transform 0.3s ease;
}
</style>
