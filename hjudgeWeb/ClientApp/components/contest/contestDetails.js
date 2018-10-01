﻿import { Get } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';

export default {
    props: ['user'],
    data: () => ({
        contest: {},
        param: {
            cid: 0,
        },
        loading: true,
        loadingProblem: true,
        page: 0,
        pageCount: 0,
        problems: [],
        verified: false,
        valid: false,
        password: '',
        passwordRules: [],
        headers: [
            {
                text: '编号',
                align: 'left',
                sortable: true,
                value: 'id'
            },
            { text: '名称', value: 'name' },
            { text: '添加时间', value: 'creationTime' },
            { text: '类型', value: 'type' },
            { text: '难度', value: 'level' },
            { text: '状态', value: 'status' },
            { text: '通过量', value: 'acceptCount' },
            { text: '提交量', value: 'submissionCount' }
        ]
    }),
    mounted: function () {
        if (this.$route.params.cid) {
            this.param.cid = parseInt(this.$route.params.cid);
        }
        setTitle('比赛详情');
        Get('/Contest/GetContestDetails', this.param)
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.contest = data;
                    this.passwordRules = [
                        v => !!v || '请输入密码',
                        v => v === this.contest.password || '密码不正确'
                    ];
                    let pwdstr = localStorage.getItem('contest_' + this.contest.id);
                    if (this.contest.password && this.contest.password !== '') {
                        if (this.contest.password === pwdstr) {
                            this.password = pwdstr;
                            this.verified = this.valid = true;
                        }
                    }
                }
                else alert(data.errorMessage);
                this.loading = false;
            })
            .catch(() => {
                alert('加载失败');
                this.loading = false;
            });
        Get('/Contest/GetProblemCount', this.param)
            .then(res => res.json())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.page > this.pageCount)
                    this.page = this.pageCount;
            })
            .catch(() => this.pageCount = 0);
        Get('/Contest/GetProblemList', this.param)
            .then(res => res.json())
            .then(data => {
                this.problems = data;
                this.loadingProblem = false;
            })
            .catch(() => {
                alert('题目列表加载失败');
                this.loadingProblem = false;
            });
    },
    watch: {
        page: function () {
            this.loadingProblem = true;
            let param = { cid: this.param.cid, start: (this.page - 1) * 10, count: 10 };
            this.problems = [];
            Get('/Contest/GetProblemList', param)
                .then(res => res.json())
                .then(data => {
                    this.problems = data;
                    this.loadingProblem = false;
                })
                .catch(() => {
                    this.problems = [];
                    this.loadingProblem = false;
                });
        }
    },
    methods: {
        toDetails: function (id) {
            this.$router.push('/ProblemDetails/' + this.param.cid + '/' + id);
        },
        verify: function () {
            if (this.password === this.contest.password) {
                this.verified = true;
                localStorage.setItem('contest_' + this.contest.id.toString(), this.password);
            }
        }
    }
};