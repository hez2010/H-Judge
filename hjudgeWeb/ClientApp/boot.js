import '@babel/polyfill';
import Vue from 'vue';
import Vuetify from 'vuetify';
import VueRouter from 'vue-router';
import 'vuetify/dist/vuetify.min.css';
import 'material-design-icons-iconfont/dist/material-design-icons.css';
import hljs from 'highlight.js';
import 'highlight.js/styles/github.css';
import 'isomorphic-fetch';

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

import home from './components/home/home.vue';
import problem from './components/problem/problem.vue';
import problemDetails from './components/problem/problemDetails.vue';
import contest from './components/contest/contest.vue';
import contestDetails from './components/contest/contestDetails.vue';
import status from './components/status/status.vue';
import result from './components/result/result.vue';
import rank from './components/rank/rank.vue';
import group from './components/group/group.vue';
import message from './components/message/message.vue';
import about from './components/about/about.vue';
import portal from './components/account/portal/portal.vue';
import user from './components/account/user/user.vue';
import problemAdmin from './components/admin/problem/problem.vue';
import contestAdmin from './components/admin/contest/contest.vue';
import groupAdmin from './components/admin/group/group.vue';
import configAdmin from './components/admin/config/config.vue';

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
