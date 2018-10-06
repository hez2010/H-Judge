import { Get } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';

export default {
    props: ['user'],
    data: () => ({
        contest: {},
        param: {
            cid: 0,
            gid: 0
        },
        loading: true,
        loadingProblem: true,
        page: 0,
        pageCount: 0,
        active: 0,
        problems: [],
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
            { text: '提交量', value: 'submissionCount' }
        ],
        timer: null
    }),
    mounted: function () {
        if (this.$route.params.cid) this.param.cid = parseInt(this.$route.params.cid);
        if (this.$route.params.gid) this.param.gid = parseInt(this.$route.params.gid);
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
                    let pwdstr = sessionStorage.getItem('contest_' + this.contest.id);
                    if (this.contest.password && this.contest.password !== '') {
                        if (this.contest.password === pwdstr) {
                            this.password = pwdstr;
                            this.verified = this.valid = true;
                        }
                    }
                    this.$nextTick(() => this.timer = setInterval(() => this.loadMath(), 200));
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
                if (this.pageCount === 0) this.pageCount = 1;
                if (this.page > this.pageCount) this.page = this.pageCount;
                if (this.page <= 0) this.page = 1;
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
                this.problems = [];
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
                    alert('题目列表加载失败');
                    this.problems = [];
                    this.loadingProblem = false;
                });
        },
        active: function () {
            if (this.active === 3) {
                this.$router.push('/Status/' + (this.param.gid ? this.param.gid.toString() + '/' : '') + this.param.cid.toString() + '/0/1');
            }
            else if (this.active === 4) {
                this.$router.push('/Rank/' + (this.param.gid ? this.param.gid.toString() + '/' : '') + this.param.cid.toString());
            }
        }
    },
    methods: {
        loadMath: function () {
            if (window.MathJax) {
                window.MathJax.Hub.Queue(['Typeset', window.MathJax.Hub]);
                clearInterval(this.timer);
            }
        },
        toDetails: function (id) {
            this.$router.push('/ProblemDetails/' + this.param.cid + '/' + id);
        },
        verify: function () {
            if (this.password === this.contest.password) {
                this.verified = true;
                sessionStorage.setItem('contest_' + this.contest.id.toString(), this.password);
            }
        }
    }
};