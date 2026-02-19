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
        path: 'home',
        name: 'home-session',
        component: () => import('@/features/session/views/HomePage.vue')
      },
      {
        path: 'users',
        name: 'users',
        component: () => import('@/features/session/views/UsersPage.vue')
      },
      {
        path: 'clients',
        name: 'clients',
        component: () => import('@/features/session/views/ClientsPage.vue')
      },
      {
        path: 'history',
        name: 'history',
        component: () => import('@/features/session/views/HistoryPage.vue')
      },
      {
        path: 'monitoring',
        name: 'monitoring',
        component: () => import('@/features/session/views/MonitoringPage.vue')
      },
      {
        path: 'customer-service',
        name: 'customer-service',
        component: () => import('@/features/session/views/CustomerServicePage.vue')
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
    
    // Verifica se o usuário tem acesso à rota pela lista de views
    const userViews = authStore.user?.views
    if (userViews && userViews.length > 0 && to.path !== '/session') {
      const hasAccess = userViews.some(v => v.route === to.path)
      if (!hasAccess) {
        toastStore.error('Acesso negado. Você não tem permissão para acessar esta página.')
        next({ name: 'home-session' })
        return
      }
    }
  }
  
  // Usuário logado tentando acessar login
  if (to.name === 'login' && hasToken) {
    next({ name: 'home' })
    return
  }
  
  next()
})

export default router
