import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import pinia from '@/stores'
import { useAuthStore } from '@/features/auth'
import { useToastStore } from '@/stores/toastStore'

const routes: RouteRecordRaw[] = [
  // Site público
  {
    path: '/',
    name: 'home',
    component: () => import('@/features/site/views/HomePage.vue'),
    meta: { requiresAuth: false }
  },

  // Auth
  {
    path: '/login',
    name: 'login',
    component: () => import('@/features/auth/views/LoginPage.vue'),
    meta: { requiresAuth: false }
  },

  // Área autenticada - /session/*
  {
    path: '/session',
    component: () => import('@/features/session/layouts/SessionLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'dashboard',
        component: () => import('@/features/session/views/DashboardPage.vue')
      },
      {
        path: 'robots',
        name: 'robots',
        component: () => import('@/features/session/views/PlaceholderPage.vue'),
        props: { title: 'Robôs', description: 'Gerencie seus robôs de automação.' }
      },
      {
        path: 'clients',
        name: 'clients',
        component: () => import('@/features/session/views/PlaceholderPage.vue'),
        props: { title: 'Clientes', description: 'Cadastro e gestão de clientes.' }
      },
      {
        path: 'reports',
        name: 'reports',
        component: () => import('@/features/session/views/PlaceholderPage.vue'),
        props: { title: 'Relatórios', description: 'Visualize relatórios e métricas.' }
      },
      {
        path: 'settings',
        name: 'settings',
        component: () => import('@/features/session/views/PlaceholderPage.vue'),
        props: { title: 'Configurações', description: 'Configure seu sistema.' }
      }
    ]
  },

  // 404
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/features/site/views/NotFoundPage.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Navigation guard para autenticação
router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore(pinia)
  const toastStore = useToastStore(pinia)
  const hasToken = !!localStorage.getItem('token')
  
  // Rota requer autenticação
  if (to.meta.requiresAuth) {
    if (!hasToken) {
      next({ name: 'login' })
      return
    }
    
    // Valida sessão no backend
    const result = await authStore.checkAuth()
    
    if (!result.valid) {
      if (result.expired) {
        toastStore.warning('Sessão expirada. Faça login novamente.')
      }
      next({ name: 'login' })
      return
    }
  }
  
  // Usuário logado tentando acessar login
  if (to.name === 'login' && hasToken) {
    next({ name: 'dashboard' })
    return
  }
  
  next()
})

export default router
