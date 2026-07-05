import { createRouter, createWebHistory } from 'vue-router';
import Login from '../components/Login.vue';
import PrincipalPage from '../components/PrincipalPage.vue';
import Contability from '../components/templates/Contability.vue';
import Debts from '../components/templates/Debts.vue';
import Tickets from '../components/templates/Tickets.vue';

const routes = [
  {
    path: '/',
    redirect: () => (localStorage.getItem('userId') ? '/principal/contability' : '/login'),
  },
  {
    path: '/login',
    name: 'login',
    component: Login,
  },
  {
    path: '/principal',
    name: 'principal',
    component: PrincipalPage,
    meta: { requiresAuth: true },
    redirect: '/principal/contability',
    children: [
      {
        path: 'contability',
        name: 'principal-contability',
        component: Contability,
      },
      {
        path: 'debts',
        name: 'principal-debts',
        component: Debts,
      },
      {
        path: 'tickets',
        name: 'principal-tickets',
        component: Tickets,
      },
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/',
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to) => {
  const hasUserId = Boolean(localStorage.getItem('userId'));

  if (to.matched.some((record) => record.meta.requiresAuth) && !hasUserId) {
    return { name: 'login' };
  }

  if (to.name === 'login' && hasUserId) {
    return { name: 'principal-contability' };
  }

  return true;
});

export default router;