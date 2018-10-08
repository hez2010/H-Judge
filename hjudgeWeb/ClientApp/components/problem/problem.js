import { setTitle } from '../../utilities/titleHelper';
import { Get, Post } from '../../utilities/requestHelper';

export default {
    props: ['user'],
    data: () => ({
        loading: true,
        page: 0,
        problems: [],
        pageCount: 0,
        headers: [
            { text: '编号', value: 'id' },
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
        setTitle('题目');
        if (!this.$route.params.page) this.$router.push('/Problem/1');
        else this.page = parseInt(this.$route.params.page);
        this.load();

        Get('/Problem/GetProblemCount')
            .then(res => res.text())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.pageCount === 0) this.pageCount = 1;
                if (this.page > this.pageCount) this.page = this.pageCount;
                if (this.page <= 0) this.page = 1;
            })
            .catch(() => this.pageCount = 0);
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
            this.$router.push('/Problem/' + this.page);
        },
        user: function () {
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
            this.problems = [];
            Get('/Problem/GetProblemList', param)
                .then(res => res.json())
                .then(data => {
                    this.problems = data;
                    this.loading = false;
                })
                .catch(() => {
                    this.problems = [];
                    this.loading = false;
                });
        },
        deleteProblem: function (id) {
            if (confirm('确定要删除此题目吗？')) {
                Post('/Admin/DeleteProblem', { id: id })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.load();
                        }
                        else alert(data.errorMessage);
                    })
                    .catch(() => alert('删除失败'));
            }
        }
    }
};