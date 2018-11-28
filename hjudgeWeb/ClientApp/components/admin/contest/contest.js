import { Get, Post } from '../../../utilities/requestHelper';
import { setTitle } from '../../../utilities/titleHelper';
import { ensureLoading } from '../../../utilities/scriptHelper';
import { initializeObjects } from '../../../utilities/initHelper';

export default {
    props: ['showSnack'],
    data: () => ({
        contest: {},
        valid: false,
        loading: true,
        requireRules: [
            v => !!v || '此项不能为空'
        ],
        submitting: false,
        markdownEnabled: false
    }),
    mounted: function () {
        setTitle('比赛编辑');

        initializeObjects({
            timer: null
        }, this);

        Get('/Admin/GetContestConfig', { cid: this.$route.params.cid })
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.contest = data;
                    this.$nextTick(() => ensureLoading('ckeditor', 'https://cdn.hjudge.com/hjudge/lib/ckeditor/ckeditor.js', this.loadEditor));
                }
                else
                    this.showSnack(data.errorMessage, 'error', 3000);
                this.loading = false;
            })
            .catch(() => {
                this.showSnack('加载失败', 'error', 3000);
                this.loading = false;
            });
    },
    destroyed: function () {
        if (window.CKEDITOR.instances.editor)
            window.CKEDITOR.instances.editor.destroy();
    },
    methods: {
        loadEditor: function () {
            this.timer = setInterval(() => {
                if (document.getElementById('editor')) {
                    clearInterval(this.timer);
                    window.CKEDITOR.replace('editor')
                        .on('markdownEnabled', obj => this.markdownEnabled = obj.data);
                    this.timer = null;
                }
            }, 1000);
        },
        isValid: function () {
            return this.valid && this.contest.startTime && this.contest.endTime;
        },
        save: function () {
            if (!this.$refs.basic.validate()) return;
            if (!this.isValid()) return;

            if (window.CKEDITOR)
                this.contest.description = window.CKEDITOR.instances['editor'].getData();

            this.submitting = true;
            Post('/Admin/UpdateContestConfig', this.contest)
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        if (this.contest.id === 0) {
                            this.$router.replace('/Admin/Contest/' + data.id.toString());
                            this.contest.id = data.id;
                        }
                        this.showSnack('保存成功', 'success', 3000);
                    }
                    else
                        this.showSnack(data.errorMessage, 'error', 3000);
                    this.submitting = false;
                })
                .catch(() => {
                    this.showSnack('保存失败', 'error', 3000);
                    this.submitting = false;
                });
        }
    }
};