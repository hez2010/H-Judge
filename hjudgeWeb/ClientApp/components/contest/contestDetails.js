﻿import { Get, Post } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';
import { ensureLoading } from '../../utilities/scriptHelper';
import { initializeObjects } from '../../utilities/initHelper';
const chatboard = () => import(/* webpackChunkName: 'chatboard' */'../chatboard/chatboard.vue');

export default {
    props: ['user', 'showSnack'],
    data: () => ({
        loading: true,
        loadingProblem: true,
        page: 0,
        pageCount: 0,
        active: 0,
        verified: false,
        valid: false,
        password: '',
        passwordRules: [],
        headers: [
            { text: '编号', value: 'id' },
            { text: '名称', value: 'name' },
            { text: '添加时间', value: 'creationTime' },
            { text: '类型', value: 'type' },
            { text: '难度', value: 'level' },
            { text: '状态', value: 'status' },
            { text: '通过量', value: 'acceptCount' },
            { text: '提交量', value: 'submissionCount' },
            { text: '比率', value: 'ratio' }
        ]
    }),
    components: {
        chatboard: chatboard
    },
    mounted: function () {
        setTitle('比赛详情');

        initializeObjects({
            contest: {},
            param: {
                cid: 0,
                gid: 0
            },
            problems: []
        }, this);

        if (this.$route.params.cid) this.param.cid = parseInt(this.$route.params.cid);
        if (this.$route.params.gid) this.param.gid = parseInt(this.$route.params.gid);
        Get('/Contest/GetContestDetails', this.param)
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.contest = data;
                    this.passwordRules = [
                        v => !!v || '请输入密码',
                        v => v === this.contest.password || '密码不正确'
                    ];
                    let pwdstr = sessionStorage.getItem('contest_' + this.contest.id);
                    if (this.contest.password && this.contest.password !== '') {
                        if (this.contest.password === pwdstr) {
                            this.password = pwdstr;
                            this.verified = this.valid = true;
                        }
                    }
                    this.$nextTick(() => ensureLoading('mathjax', 'https://cdn.hjudge.com/hjudge/lib/MathJax-2.7.5/MathJax.js?config=TeX-MML-AM_CHTML', this.loadMath));
                }
                else this.showSnack(data.errorMessage, 'error', 3000);
                this.loading = false;
            })
            .catch(() => {
                this.showSnack('加载失败', 'error', 3000);
                this.loading = false;
            });
        Get('/Contest/GetProblemCount', this.param)
            .then(res => res.json())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.pageCount === 0) this.pageCount = 1;
                if (this.page > this.pageCount) this.page = this.pageCount;
                if (this.page <= 0) this.page = 1;
            })
            .catch(() => this.pageCount = 0);
        this.loadProblems();
    },
    watch: {
        page: function () {
            this.loadProblems();
        },
        active: function () {
            if (this.active === 1) {
                if (window['MathJax']) this.loadMath();
            }
            else if (this.active === 3) {
                this.$router.push('/Status/' + (this.param.gid ? this.param.gid.toString() + '/' : '') + this.param.cid.toString() + '/0/1');
            }
            else if (this.active === 4) {
                this.$router.push('/Rank/' + (this.param.gid ? this.param.gid.toString() + '/' : '') + this.param.cid.toString());
            }
        }
    },
    methods: {
        loadProblems: function () {
            this.loadingProblem = true;
            let param = { cid: this.param.cid, start: (this.page - 1) * 10, count: 10 };
            this.problems = [];
            Get('/Contest/GetProblemList', param)
                .then(res => res.json())
                .then(data => {
                    this.problems = data.map(v => {
                        v['ratio'] = v.submissionCount === 0 ? 0 : Math.round(v.acceptCount * 10000 / v.submissionCount) / 100;
                        return v;
                    });
                    this.loadingProblem = false;
                })
                .catch(() => {
                    this.showSnack('题目列表加载失败', 'error', 3000);
                    this.problems = [];
                    this.loadingProblem = false;
                });
        },
        deleteContest: function (id) {
            if (confirm('确定要删除此比赛吗？')) {
                Post('/Admin/DeleteContest', { id: id })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.showSnack('删除成功', 'success', 3000);
                            this.$router.go(-1);
                        }
                        else this.showSnack(data.errorMessage, 'error', 3000);
                    })
                    .catch(() => this.showSnack('删除失败', 'error', 3000));
            }
        },
        loadMath: function () {
            this.$nextTick(() => {
                if (document.getElementById('details_content')) {
                    window.MathJax.Hub.Queue(['Typeset', window.MathJax.Hub]);
                }
            });
        },
        verify: function () {
            if (this.password === this.contest.password) {
                this.verified = true;
                sessionStorage.setItem('contest_' + this.contest.id.toString(), this.password);
            }
        }
    }
};