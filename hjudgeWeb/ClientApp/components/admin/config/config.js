import { Get, Post } from '../../../utilities/requestHelper';
import { setTitle } from '../../../utilities/titleHelper';
import { initializeObjects } from '../../../utilities/initHelper';

export default {
    props: ['showSnack'],
    data: () => ({
        loading: true,
        valid: false,
        requireRules: [
            v => !!v || '请填写该项内容'
        ],
        submitting: false
    }),
    mounted: function () {
        setTitle('设置');

        initializeObjects({
            config: {},
            supportLanguages: ['abap', 'abc', 'actionscript', 'ada', 'apache_conf', 'apex', 'applescript', 'asciidoc', 'asl', 'assembly_x86', 'autohotkey', 'batchfile', 'bro', 'c9search', 'cirru', 'clojure', 'cobol', 'coffee', 'coldfusion', 'csharp', 'csound_document', 'csound_orchestra', 'csound_score', 'csp', 'css', 'curly', 'c_cpp', 'd', 'dart', 'diff', 'django', 'dockerfile', 'dot', 'drools', 'edifact', 'eiffel', 'ejs', 'elixir', 'elm', 'erlang', 'forth', 'fortran', 'fsharp', 'fsl', 'ftl', 'gcode', 'gherkin', 'gitignore', 'glsl', 'gobstones', 'golang', 'graphqlschema', 'groovy', 'haml', 'handlebars', 'haskell', 'haskell_cabal', 'haxe', 'hjson', 'html', 'html_elixir', 'html_ruby', 'ini', 'io', 'jack', 'jade', 'java', 'javascript', 'json', 'jsoniq', 'jsp', 'jssm', 'jsx', 'julia', 'kotlin', 'latex', 'less', 'liquid', 'lisp', 'livescript', 'logiql', 'logtalk', 'lsl', 'lua', 'luapage', 'lucene', 'makefile', 'markdown', 'mask', 'matlab', 'maze', 'mel', 'mixal', 'mushcode', 'mysql', 'nix', 'nsis', 'objectivec', 'ocaml', 'pascal', 'perl', 'perl6', 'pgsql', 'php', 'php_laravel_blade', 'pig', 'plain_text', 'powershell', 'praat', 'prolog', 'properties', 'protobuf', 'puppet', 'python', 'r', 'razor', 'rdoc', 'red', 'redshift', 'rhtml', 'rst', 'ruby', 'rust', 'sass', 'scad', 'scala', 'scheme', 'scss', 'sh', 'sjs', 'slim', 'smarty', 'snippets', 'soy_template', 'space', 'sparql', 'sql', 'sqlserver', 'stylus', 'svg', 'swift', 'tcl', 'terraform', 'tex', 'text', 'textile', 'toml', 'tsx', 'turtle', 'twig', 'typescript', 'vala', 'vbscript', 'velocity', 'verilog', 'vhdl', 'visualforce', 'wollok', 'xml', 'xquery', 'yaml']
        }, this);
        
        Get('/Admin/GetSystemConfig')
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.config = data;
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
    methods: {
        submit: function () {
            if (this.$refs.form.validate()) {
                this.submitting = true;
                Post('/Admin/UpdateSystemConfig', this.config)
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded)
                            this.showSnack('保存成功', 'success', 3000);
                        else
                            this.showSnack(data.errorMessage, 'error', 3000);
                        this.submitting = false;
                    })
                    .catch(() => {
                        this.showSnack('保存失败', 'error', 3000);
                        this.submitting = false;
                    });
            }
        },
        add: function () {
            this.config.languages = this.config.languages.concat([{
                name: '',
                extensions: '',
                information: '',
                syntaxHighlight: '',
                compilerExec: '',
                compilerArgs: '',
                compilerProblemMatcher: '',
                compilerDisplayFormat: '',
                compilerReadStdOutput: true,
                compilerReadStdError: true,
                staticCheckExec: '',
                staticCheckArgs: '',
                staticCheckProblemMatcher: '',
                staticCheckDisplayFormat: '',
                staticCheckReadStdOutput: true,
                staticCheckReadStdError: true,
                runExec: '',
                runArgs: ''
            }]);
            this.$forceUpdate();
        },
        remove: function (index) {
            this.config.languages.splice(index, 1);
            this.$forceUpdate();
        }
    }
};