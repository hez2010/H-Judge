import Vue from 'vue';
import Vuetify from 'vuetify';
import VueRouter from 'vue-router';
import hljs from 'highlight.js';

Vue.use(VueRouter);
Vue.use(Vuetify);
Vue.directive('highlight', function (el) {
    const blocks = el.querySelectorAll('pre code');
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
                    if (this.readyState == 'compconste') {
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

const home = () => import(/* webpackChunkName: 'home' */'./components/home/home.vue');
const problem = () => import(/* webpackChunkName: 'problem' */'./components/problem/problem.vue');
const problemDetails = () => import(/* webpackChunkName: 'problemDetails' */'./components/problem/problemDetails.vue');
const contest = () => import(/* webpackChunkName: 'contest' */'./components/contest/contest.vue');
const contestDetails = () => import(/* webpackChunkName: 'contestDetails' */'./components/contest/contestDetails.vue');
const status = () => import(/* webpackChunkName: 'status' */'./components/status/status.vue');
const result = () => import(/* webpackChunkName: 'result' */'./components/result/result.vue');
const rank = () => import(/* webpackChunkName: 'rank' */'./components/rank/rank.vue');
const group = () => import(/* webpackChunkName: 'group' */'./components/group/group.vue');
const message = () => import(/* webpackChunkName: 'message' */'./components/message/message.vue');
const about = () => import(/* webpackChunkName: 'about' */'./components/about/about.vue');
const portal = () => import(/* webpackChunkName: 'portal' */'./components/account/portal/portal.vue');
const user = () => import(/* webpackChunkName: 'user' */'./components/account/user/user.vue');
const problemAdmin = () => import(/* webpackChunkName: 'problemAdmin' */'./components/admin/problem/problem.vue');
const contestAdmin = () => import(/* webpackChunkName: 'contestAdmin' */'./components/admin/contest/contest.vue');
const groupAdmin = () => import(/* webpackChunkName: 'groupAdmin' */'./components/admin/group/group.vue');
const configAdmin = () => import(/* webpackChunkName: 'configAdmin' */'./components/admin/config/config.vue');

const routes = [
    { path: '/', component: home },

    { path: '/Problem/:page', component: problem },
    { path: '/Problem', component: problem },

    { path: '/ProblemDetails/:gid/:cid/:pid', component: problemDetails },
    { path: '/ProblemDetails/:cid/:pid', component: problemDetails },
    { path: '/ProblemDetails/:pid', component: problemDetails },

    { path: '/Contest/:page', component: contest },
    { path: '/Contest', component: contest },

    { path: '/ContestDetails/:gid/:cid', component: contestDetails },
    { path: '/ContestDetails/:cid', component: contestDetails },

    { path: '/Status/:gid/:cid/:pid/:page', component: status },
    { path: '/Status/:cid/:pid/:page', component: status },
    { path: '/Status/:pid/:page', component: status },
    { path: '/Status/:page', component: status },
    { path: '/Status', component: status },

    { path: '/Result/:jid', component: result },

    { path: '/Rank/:gid/:cid', component: rank },
    { path: '/Rank/:cid', component: rank },

    { path: '/Group/:page', component: group },
    { path: '/Group', component: group },

    { path: '/Message', component: message },

    { path: '/About', component: about },

    { path: '/Account', component: portal },

    { path: '/Account/:uid', component: user },

    { path: '/Admin/Problem/:pid', component: problemAdmin },
    { path: '/Admin/Contest/:cid', component: contestAdmin },
    { path: '/Admin/Group/:gid', component: groupAdmin },
    { path: '/Admin/Config', component: configAdmin }
];

new Vue({
    el: '#app-root',
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(require('./components/app/app.vue').default)
});
