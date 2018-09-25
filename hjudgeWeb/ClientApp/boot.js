import 'babel-polyfill';
import Vue from 'vue';
import Vuetify from 'vuetify';
import VueRouter from 'vue-router';
import 'vuetify/dist/vuetify.min.css';
import 'material-design-icons-iconfont/dist/material-design-icons.css';

Vue.use(VueRouter);
Vue.use(Vuetify);

const routes = [
    { path: '/', component: require('./components/home/home.vue').default },
    { path: '/Problem', component: require('./components/problem/problem.vue').default },
    { path: '/Contest', component: require('./components/contest/contest.vue').default },
    { path: '/Status', component: require('./components/status/status.vue').default },
    { path: '/Group', component: require('./components/group/group.vue').default },
    { path: '/Account', component: require('./components/account/portal/portal.vue').default }
];

new Vue({
    el: '#app-root',
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(require('./components/app/app.vue').default)
});
