<script setup lang="ts">
defineProps<{
  title: string
  description?: string
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center h-full text-center">
    <div class="w-20 h-20 bg-gray-200 dark:bg-gray-700 rounded-2xl flex items-center justify-center mb-6">
      <span class="text-4xl">🚧</span>
    </div>
    <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
      {{ title }}
    </h2>
    <p class="text-gray-500 dark:text-gray-400 max-w-md">
      {{ description || 'Esta página está em construção. Em breve teremos novidades!' }}
    </p>
  </div>
</template>
