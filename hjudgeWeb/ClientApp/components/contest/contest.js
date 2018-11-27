import { setTitle } from '../../utilities/titleHelper';
import { Get, Post } from '../../utilities/requestHelper';
import { initializeObjects } from '../../utilities/initHelper';

export default {
    props: ['user', 'showSnack'],
    data: () => ({
        loading: true,
        page: 0,
        pageCount: 0,
        headers: [
            { text: '编号', value: 'id' },
            { text: '名称', value: 'name' },
            { text: '开始时间', value: 'startTime' },
            { text: '结束时间', value: 'endTime' },
            { text: '题目数量', value: 'problemCount' },
            { text: '状态', value: 'status' }
        ]
    }),
    mounted: function () {
        setTitle('比赛');
        initializeObjects({
            contests: []
        }, this);

        if (!this.$route.params.page) this.$router.push('/Contest/1');
        else this.page = parseInt(this.$route.params.page);
        this.load();
        Get('/Contest/GetContestCount')
            .then(res => res.text())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.pageCount === 0) this.pageCount = 1;
                if (this.page > this.pageCount) this.page = this.pageCount;
                if (this.page <= 0) this.page = 1;
            })
            .catch(() => this.pageCount = 0);
        this.headers.splice(6);
        if (this.user && this.user.privilege >= 1 && this.user.privilege <= 2) {
            this.headers = this.headers.concat([{ text: '操作', value: 'actions', sortable: false }]);
        }
    },
    watch: {
        $route: function () {
            let page = this.$route.params.page ? parseInt(this.$route.params.page) : 1;
            this.page = page;
            this.load();
        },
        page: function () {
            this.$router.push('/Contest/' + this.page);
        },
        user: function () {
            this.headers.splice(6);
            if (this.user && this.user.privilege >= 1 && this.user.privilege <= 2) {
                this.headers = this.headers.concat([{ text: '操作', value: 'actions', sortable: false }]);
            }
        }
    },
    methods: {
        load: function () {
            if (this.page === 0) return;
            this.loading = true;
            let param = { start: (this.page - 1) * 10, count: 10 };
            if (this.$route.params.gid) param['gid'] = this.$route.params.gid;
            this.contests = [];
            Get('/Contest/GetContestList', param)
                .then(res => res.json())
                .then(data => {
                    this.contests = data;
                    this.loading = false;
                })
                .catch(() => {
                    this.contests = [];
                    this.loading = false;
                    this.showSnack('加载失败', 'error', 3000);
                });
        },
        deleteContest: function (id) {
            if (confirm('确定要删除此比赛吗？')) {
                Post('/Admin/DeleteContest', { id: id })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.showSnack('删除成功', 'success', 3000);
                            this.load();
                        }
                        else this.showSnack(data.errorMessage, 'error', 3000);
                    })
                    .catch(() => this.showSnack('删除失败', 'error', 3000));
            }
        },
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
    }
};