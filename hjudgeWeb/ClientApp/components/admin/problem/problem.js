import { Get, Post, ReadCookie } from '../../../utilities/requestHelper';
import { setTitle } from '../../../utilities/titleHelper';

export default {
    data: () => ({
        problem: {},
        valid_basic: false,
        valid_answer: false,
        valid_points: false,
        loading: true,
        uploading: false,
        uploadingText: '上传题目数据',
        requireRules: [
            v => !!v || '此项不能为空'
        ],
        dataTemplate: [
            '${datadir}/${name}${index}.in|${datadir}/${name}${index}.ans|1000|131072|10',
            '${datadir}/${name}${index}.in|${datadir}/${name}${index}.out|1000|131072|10',
            '${datadir}/${name}${index0}.in|${datadir}/${name}${index0}.ans|1000|131072|10',
            '${datadir}/${name}${index0}.in|${datadir}/${name}${index0}.out|1000|131072|10'
        ],
        templateSelection: '${datadir}/${name}${index}.in|${datadir}/${name}${index}.ans|1000|131072|10',
        timer: null,
        submitting: false
    }),
    mounted: function () {
        setTitle('题目编辑');
        Get('/Admin/GetProblemConfig', { pid: this.$route.params.pid })
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.problem = data;
                    this.$nextTick(() => this.timer = setInterval(() => this.loadEditor(), 200));
                }
                else alert(data.errorMessage);
                this.loading = false;
            })
            .catch(() => {
                alert('加载失败');
                this.loading = false;
            });
    },
    methods: {
        addPoint: function () {
            this.problem.config.points = this.problem.config.points.concat([{ stdInFile: '', stdOutFile: '', timeLimit: 1000, memoryLimit: 131072, score: 10 }]);
        },
        removePoint: function (index) {
            this.problem.config.points.splice(index, 1);
        },
        applyTemplate: function () {
            let args = this.templateSelection.split('|');
            if (args.length === 5) {
                for (var point in this.problem.config.points) {
                    let index = 0;
                    for (var item in this.problem.config.points[point]) {
                        if (args[index] !== '*') this.problem.config.points[point][item] = args[index];
                        index++;
                    }
                }
            }
        },
        loadEditor: function () {
            if (window.CKEDITOR) {
                clearInterval(this.timer);
                window.CKEDITOR.replace('editor');
            }
        },
        valid: function () {
            return (this.valid_basic && (this.problem.type === 1 ? this.valid_points : this.valid_answer));
        },
        save: function () {
            if (!this.$refs.basic.validate()) return;
            if (this.problem.type === 1) {
                if (!this.$refs.points.validate()) return;
            } else {
                if (!this.$refs.answer.validate()) return;
            }
            if (this.problem.config.extraFilesText) this.problem.config.extraFiles = this.problem.config.extraFilesText.split('\n');

            if (window.CKEDITOR)
                this.problem.description = window.CKEDITOR.instances['editor'].getData();

            this.submitting = true;
            Post('/Admin/UpdateProblemConfig', this.problem)
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        if (this.problem.id === 0) {
                            this.$router.push('/Admin/Problem/' + data.id.toString());
                            this.problem.id = data.id;
                        }
                        alert('保存成功');
                    }
                    else alert(data.errorMessage);
                    this.submitting = false;
                })
                .catch(() => {
                    alert('保存失败');
                    this.submitting = false;
                });
        },
        uploadData: function () {
            let ele = document.getElementById('data_file');
            let file = ele.files[0];
            if (file.type !== 'application/x-zip-compressed' && file.type !== 'application/zip') {
                alert('文件格式不正确');
                ele.value = '';
                return;
            }
            if (file.size > 134217728) {
                alert('文件大小不能超过 128 Mb');
                ele.value = '';
                return;
            }
            let form = new FormData();
            form.append('pid', this.problem.id);
            form.append('file', file);
            this.uploadingText = '上传中...';
            this.uploading = true;
            let token = ReadCookie('XSRF-TOKEN');
            fetch('/Admin/UploadProblemData', {
                method: 'POST',
                credentials: 'same-origin',
                body: form,
                mode: 'cors',
                cache: 'default',
                headers: { 'X-XSRF-TOKEN': token }
            })
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) alert('上传成功');
                    else alert(data.errorMessage);
                    this.uploadingText = '上传题目数据';
                    this.uploading = false;
                    ele.value = '';
                })
                .catch(() => {
                    alert('上传失败');
                    this.uploadingText = '上传题目数据';
                    this.uploading = false;
                    ele.value = '';
                });
        },
        downloadData: function () {
            let link = document.createElement('a');
            link.href = '/Admin/DownloadProblemData/' + this.problem.id;
            link.target = '_blank';
            link.click();
            link.remove();
        },
        deleteData: function () {
            if (confirm('确定要删除题目数据吗？')) {
                Post('/Admin/DeleteProblemData', { id: this.problem.id })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            alert('删除成功');
                        }
                        else alert(data.errorMessage);
                    })
                    .catch(() => alert('删除失败'));
            }
        },
        selectFile: function () {
            document.getElementById('data_file').click();
        },
    }
};