import { setTitle } from '../../utilities/titleHelper';
import { Get } from '../../utilities/requestHelper';

export default {
    props: ['user'],
    data: () => ({
        loading: true,
        page: 0,
        param: { start: 0, count: 10 },
        problems: [],
        pageCount: 0,
        statuses: [],
        headers: [
            { text: '编号', value: 'id' },
            { text: '时间', value: 'judgeTime' },
            { text: '题目', value: 'problemName' },
            { text: '用户', value: 'userName' },
            { text: '语言', value: 'language' },
            { text: '结果', value: 'result' },
            { text: '总分', value: 'fullScore' }
        ],
        onlyMe: false,
        sortRules: function (items, index, isDescending) {
            if (index === 'id') {
                return items.sort((x, y) => {
                    if (x.id < y.id) return isDescending ? -1 : 1;
                    else if (x.id > y.id) return isDescending ? 1 : -1;
                    else return 0;
                });
            } else {
                return items.sort((x, y) => {
                    if (x.id < y.id) return isDescending ? 1 : -1;
                    else if (x.id > y.id) return isDescending ? -1 : 1;
                    else return 0;
                });
            }
        }
    }),
    mounted: function () {
        setTitle('状态');
        if (!this.$route.params.page) this.$router.push(this.$router.currentRoute.fullPath + '/1');
        else this.page = parseInt(this.$route.params.page);

        if (this.$route.params.pid) this.param['pid'] = parseInt(this.$route.params.pid);
        if (this.$route.params.cid) {
            this.headers = this.headers.concat([{ text: '比赛', value: 'contestId' }]);
            this.param['cid'] = parseInt(this.$route.params.cid);
        }
        if (this.$route.params.gid) {
            this.headers = this.headers.concat([{ text: '小组', value: 'groupId' }]);
            this.param['gid'] = parseInt(this.$route.params.gid);
        }
        this.load();

        this.param['onlyMe'] = this.onlyMe;
        Get('/Status/GetStatusCount', this.param)
            .then(res => res.text())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.pageCount === 0) this.pageCount = 1;
                if (this.page > this.pageCount) this.page = this.pageCount;
                if (this.page <= 0) this.page = 1;
            })
            .catch(() => this.pageCount = 0);
    },
    watch: {
        $route: function () {
            let page = this.$route.params.page ? parseInt(this.$route.params.page) : 1;
            this.page = page;
            this.load();
        },
        page: function () {
            let baseRoute = '/Status';
            if (this.$route.params.gid) baseRoute = baseRoute + '/' + this.$route.params.gid;
            if (this.$route.params.cid) baseRoute = baseRoute + '/' + this.$route.params.cid;
            if (this.$route.params.pid) baseRoute = baseRoute + '/' + this.$route.params.pid;
            this.$router.push(baseRoute + '/' + this.page);
        },
        onlyMe: function () {
            this.param['onlyMe'] = this.onlyMe;
            Get('/Status/GetStatusCount', this.param)
                .then(res => res.text())
                .then(data => {
                    this.pageCount = Math.ceil(data / 10);
                    if (this.pageCount === 0) this.pageCount = 1;
                    if (this.page > this.pageCount) this.page = this.pageCount;
                    if (this.page <= 0) this.page = 1;
                })
                .catch(() => this.pageCount = 0);
            this.load();
        }
    },
    methods: {
        getProblemRouteParams: function (id) {
            return (this.$route.params.gid ? `${this.$route.params.gid}/` : '') + (this.$route.params.cid ? `${this.$route.params.cid}/` : '') + id.toString();
        },
        getContestRouteParams: function (id) {
            return (this.$route.params.gid ? `${this.$route.params.gid}/` : '') + id.toString();
        },
        load: function () {
            if (this.page === 0) return;
            this.loading = true;
            this.param.start = (this.page - 1) * 10;
            this.statuses = [];
            this.param['onlyMe'] = this.onlyMe;
            Get('/Status/GetStatusList', this.param)
                .then(res => res.json())
                .then(data => {
                    this.statuses = data;
                    this.loading = false;
                })
                .catch(() => {
                    this.statuses = [];
                    this.loading = false;
                });
        }
    }
};