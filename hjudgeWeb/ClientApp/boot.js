import 'babel-polyfill';
import Vue from 'vue';
import Vuetify from 'vuetify';
import VueRouter from 'vue-router';
import 'vuetify/dist/vuetify.min.css';
import 'material-design-icons-iconfont/dist/material-design-icons.css';
import hljs from 'highlight.js';
import 'highlight.js/styles/github.css';

Vue.use(VueRouter);
Vue.use(Vuetify);
Vue.directive('highlight', function (el) {
    let blocks = el.querySelectorAll('pre code');
    blocks.forEach((block) => {
        hljs.highlightBlock(block);
    });
});

Vue.component('remote-script', {
    render: function (createElement) {
        var self = this;
        return createElement('script', {
            attrs: {
                type: 'text/javascript',
                src: this.src
            },
            on: {
                load: function (event) {
                    self.$emit('load', event);
                },
                error: function (event) {
                    self.$emit('error', event);
                },
                readystatechange: function (event) {
                    if (this.readyState == 'complete') {
                        self.$emit('load', event);
                    }
                }
            }
        });
    },
    props: {
        src: {
            type: String,
            required: true
        }
    }
});

const routes = [
    { path: '/', component: require('./components/home/home.vue').default },
    { path: '/Problem/:page', component: require('./components/problem/problem.vue').default },
    { path: '/Problem', component: require('./components/problem/problem.vue').default },
    { path: '/ProblemDetails/:gid/:cid/:pid', component: require('./components/problem/problemDetails.vue').default },
    { path: '/ProblemDetails/:cid/:pid', component: require('./components/problem/problemDetails.vue').default },
    { path: '/ProblemDetails/:pid', component: require('./components/problem/problemDetails.vue').default },
    { path: '/Contest/:page', component: require('./components/contest/contest.vue').default },
    { path: '/Contest', component: require('./components/contest/contest.vue').default },
    { path: '/ContestDetails/:gid/:cid', component: require('./components/contest/contestDetails.vue').default },
    { path: '/ContestDetails/:cid', component: require('./components/contest/contestDetails.vue').default },
    { path: '/Status/:gid/:cid/:pid/:page', component: require('./components/status/status.vue').default },
    { path: '/Status/:cid/:pid/:page', component: require('./components/status/status.vue').default },
    { path: '/Status/:pid/:page', component: require('./components/status/status.vue').default },
    { path: '/Status/:page', component: require('./components/status/status.vue').default },
    { path: '/Status', component: require('./components/status/status.vue').default },
    { path: '/Result/:jid', component: require('./components/result/result.vue').default },
    { path: '/Rank/:gid/:cid', component: require('./components/rank/rank.vue').default },
    { path: '/Rank/:cid', component: require('./components/rank/rank.vue').default },
    { path: '/Group/:page', component: require('./components/group/group.vue').default },
    { path: '/Group', component: require('./components/group/group.vue').default },
    { path: '/Message', component: require('./components/message/message.vue').default },
    { path: '/About', component: require('./components/about/about.vue').default },
    { path: '/Account', component: require('./components/account/portal/portal.vue').default },
    { path: '/Account/:uid', component: require('./components/account/user/user.vue').default },
    { path: '/Admin/Problem/:pid', component: require('./components/admin/problem/problem.vue').default },
    { path: '/Admin/Contest/:cid', component: require('./components/admin/contest/contest.vue').default },
    { path: '/Admin/Group/:gid', component: require('./components/admin/group/group.vue').default },
    { path: '/Admin/Config', component: require('./components/admin/config/config.vue').default }
];

new Vue({
    el: '#app-root',
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(require('./components/app/app.vue').default)
});
