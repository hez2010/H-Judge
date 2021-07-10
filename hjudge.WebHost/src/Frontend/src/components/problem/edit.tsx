import * as React from 'react';
import { CommonProps } from '../../interfaces/commonProps';
import { setTitle } from '../../utils/titleHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { Get, Put, Post, Delete } from '../../utils/requestHelper';
import { Placeholder, Tab, Grid, Form, Rating, Header, Button, Divider, List, Label, Segment, Icon, Confirm, Loader } from 'semantic-ui-react';
import MarkdownViewer from '../viewer/markdown';
import { tryJson } from '../../utils/responseHelper';
import AppContext from '../../AppContext';

interface ProblemDataUploadModel {
  failedFiles: string[]
}

interface ProblemEditProps {
  problemId?: number
}

interface DataPoint {
  stdInFile: string,
  stdOutFile: string,
  timeLimit: number,
  memoryLimit: number,
  score: number,
  index: number
}

interface AnswerPoint {
  answerFile: string,
  score: number
}

interface ComparingOptions {
  ignoreLineTailWhiteSpaces: boolean,
  ignoreTextTailLineFeeds: boolean
}

interface ProblemConfig {
  specialJudge: string,
  inputFileName: string,
  outputFileName: string,
  sourceFiles: string[],
  extraFiles: string[],
  points: DataPoint[],
  answer: AnswerPoint,
  comparingOptions: ComparingOptions,
  useStdIO: boolean,
  compileArgs: string,
  languages: string,
  codeSizeLimit: number
}

interface ProblemEditModel {
  id: number,
  name: string,
  level: number,
  description: string,
  type: number,
  hidden: boolean,
  config: ProblemConfig
}

interface SourceModel {
  index: number,
  fileName: string
}

const ProblemEdit = (props: ProblemEditProps & CommonProps) => {
  const [problem, setProblem] = React.useState<ProblemEditModel>({
    config: {
      answer: {
        answerFile: '',
        score: 0
      },
      codeSizeLimit: 0,
      comparingOptions: {
        ignoreLineTailWhiteSpaces: true,
        ignoreTextTailLineFeeds: true
      },
      compileArgs: '',
      extraFiles: [],
      inputFileName: '',
      languages: '',
      outputFileName: '',
      points: [],
      specialJudge: '',
      sourceFiles: [],
      useStdIO: true
    },
    description: '',
    hidden: false,
    id: 0,
    level: 1,
    name: '',
    type: 1
  });
  const [useSpecialJudge, setUseSpecialJudge] = React.useState(false);
  const [selectedTemplate, setSelectedTemplate] = React.useState('');
  const [processingData, setProcessingData] = React.useState(false);
  const [confirmOpen, setConfirmOpen] = React.useState(false);
  const [loaded, setLoaded] = React.useState(false);
  const [sources, setSources] = React.useState<SourceModel[]>([]);

  const problemId = React.useRef(0);
  const pointsCount = React.useRef(0);
  const sourceCount = React.useRef(0);
  const editor = React.useRef(React.createRef<any>());
  const fileLoader = React.useRef(React.createRef<HTMLInputElement>());

  const { userInfo, commonFuncs } = React.useContext(AppContext);

  const fetchConfig = (problemId: number) => {
    if (problemId === 0) {
      setLoaded(true);
      return;
    }

    Get('/problem/edit', { problemId: problemId })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ProblemEditModel;
        result.config.points = result.config.points.map(v => { v.index = ++pointsCount.current; return v; });
        setProblem(result);
        setUseSpecialJudge(!!result.config.specialJudge);
        setSources(result.config.sourceFiles.map(v => ({ index: ++sourceCount.current, fileName: v })));
        setLoaded(true);
      })
      .catch(err => {
        commonFuncs?.openPortal('错误', '题目配置加载失败', 'red');
        console.log(err);
      });
  }

  React.useEffect(() => {
    setTitle('题目编辑');

    if (props.problemId) problemId.current = props.problemId;
    else if (props.match.params.problemId) problemId.current = parseInt(props.match.params.problemId);
    else problemId.current = 0;

    fetchConfig(problemId.current);

    let global = (globalThis as any);
    if (global.ace) global.ace.config.set('basePath', '/lib/ace');
  }, []);


  const viewFileList = () => {
    let link = document.createElement('a');
    link.href = `/problem/data-view?problemId=${problem.id}`;
    link.target = '_blank';
    link.click();
    link.remove();
  }

  const renderPreview = () => {
    let editorTmp = editor.current.current;
    if (!editorTmp) return;
    problem.description = editorTmp.editor.getValue();
    setProblem(problem);
  }

  const submitChange = () => {
    if (!canSubmit()) {
      commonFuncs?.openPortal('错误', '题目信息填写不完整', 'red');
      return;
    }

    problem.config.sourceFiles = sources.map(v => v.fileName);
    if (problem.id === 0) {
      Put('/problem/edit', problem)
        .then(tryJson)
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          let result = data as ProblemEditModel;
          result.config.points = result.config.points.map(v => { v.index = ++pointsCount.current; return v; });
          setProblem(result);
          setSources(result.config.sourceFiles.map(v => ({ index: ++sourceCount.current, fileName: v })));
          setUseSpecialJudge(!!result.config.specialJudge);

          commonFuncs?.openPortal('成功', '题目保存成功', 'green');
          props.history.replace(`/edit/problem/${result.id}`);
        })
        .catch(err => {
          commonFuncs?.openPortal('错误', '题目保存失败', 'red');
          console.log(err);
        });
    }
    else {
      Post('/problem/edit', problem)
        .then(tryJson)
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          commonFuncs?.openPortal('成功', '题目保存成功', 'green');
        })
        .catch(err => {
          commonFuncs?.openPortal('错误', '题目配置加载失败', 'red');
          console.log(err);
        });
    }
  }

  const canSubmit = () => {
    let { config } = problem;

    let result = true;
    result = result && !!problem.name;
    if (!config.useStdIO) {
      result = result && !!config.inputFileName;
      result = result && !!config.outputFileName;
    }
    if (sources.length === 0) result = false;
    for (let i of sources) {
      if (!i.fileName) {
        result = false;
        break;
      }
    }

    if (problem.type === 1) {
      for (let x in config.points) {
        result = result && !!config.points[x].stdInFile;
        result = result && !!config.points[x].stdOutFile;
      }
      if (useSpecialJudge) result = result && !!config.specialJudge;
    }
    else {
      result = result && !!config.answer.answerFile;
    }

    return result;
  }

  const removePoint = (index: number) => () => {
    problem.config.points.splice(index, 1);
    setProblem(problem);
  };

  const addPoint = () => {
    problem.config.points = [...problem.config.points, { stdInFile: '', stdOutFile: '', timeLimit: 1000, memoryLimit: 131072, score: 10, index: ++pointsCount.current }];
    setProblem(problem);
  }

  const removeSource = (index: number) => () => {
    if (sources.length <= 1) {
      commonFuncs?.openPortal('错误', '至少要有一个提交内容', 'red');
      return;
    }

    sources.splice(index, 1);
    setSources(sources);
  };

  const addSource = () => {
    setSources([...sources, { index: ++sourceCount.current, fileName: '${random}${extension}' }]);
  }

  const applyTemplate = () => {
    const fields = selectedTemplate.split('|');
    if (fields.length !== 5) {
      commonFuncs?.openPortal('错误', '快速套用模板格式错误', 'red');
      return;
    }
    const [input, output, time, memory, score] = fields;
    const points = problem.config.points;
    for (let i in points) {
      points[i].index = ++pointsCount.current;
      if (input !== '*') points[i].stdInFile = input;
      if (output !== '*') points[i].stdOutFile = output;
      if (time !== '*') points[i].timeLimit = parseInt(time);
      if (memory !== '*') points[i].memoryLimit = parseInt(memory);
      if (score !== '*') points[i].score = parseInt(score);
    }
    setProblem(problem);
  }

  const uploadFile = () => {
    let ele = fileLoader.current.current;
    if (!ele || !ele.files || ele.files.length === 0) return;
    let file = ele.files[0];
    if (file.type !== 'application/x-zip-compressed' && file.type !== 'application/zip') {
      commonFuncs?.openPortal('错误', '文件格式不正确', 'red');
      ele.value = '';
      return;
    }
    if (file.size > 134217728) {
      commonFuncs?.openPortal('错误', '文件大小不能超过 128 Mb', 'red');
      ele.value = '';
      return;
    }
    let form = new FormData();
    form.append('problemId', problem.id.toString());
    form.append('file', file);
    setProcessingData(true);

    Put('/problem/data', form, false, '')
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
        else {
          let result = data as ProblemDataUploadModel;
          if (result.failedFiles && result.failedFiles.length !== 0)
            commonFuncs?.openPortal('警告', `部分题目数据上传失败：\n${result.failedFiles.reduce((accu, next) => accu + '\n' + next)}`, 'orange');
          else commonFuncs?.openPortal('成功', '题目数据上传成功', 'green');
        }
        setProcessingData(false);
        let ele = fileLoader.current.current;
        if (ele) ele.value = '';
      })
      .catch(() => {
        commonFuncs?.openPortal('错误', '题目数据上传失败', 'red');
        setProcessingData(false);
        let ele = fileLoader.current.current;
        if (ele) ele.value = '';
      });
  }

  const selectFile = () => {
    if (fileLoader.current.current) {
      fileLoader.current.current.click();
    }
  }

  const downloadFile = () => {
    let link = document.createElement('a');
    link.href = `/problem/data?problemId=${problem.id}`;
    link.target = '_blank';
    link.click();
    link.remove();
  }

  const deleteFile = () => {
    setConfirmOpen(false);
    setProcessingData(true);
    Delete('/problem/data', { problemId: problem.id })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
        else commonFuncs?.openPortal('成功', '题目数据删除成功', 'green');
        setProcessingData(false);
      })
      .catch(err => {
        commonFuncs?.openPortal('错误', '题目数据删除失败', 'red');
        setProcessingData(false);
        console.log(err);
      });
  }

  const render = () => {
      const placeHolder = <Placeholder>
        <Placeholder.Paragraph>
          <Placeholder.Line />
          <Placeholder.Line />
          <Placeholder.Line />
          <Placeholder.Line />
        </Placeholder.Paragraph>
      </Placeholder>;
      if (!loaded) return placeHolder;

      const basic = <Form>
        <Form.Field required error={!problem.name}>
          <label>题目名称</label>
          <Form.Input required defaultValue={problem.name} onChange={e => { problem.name = e.target.value; setProblem(problem); }} />
        </Form.Field>
        <Form.Field>
          <label>题目难度</label>
          <Rating icon='star' defaultRating={problem.level} maxRating={10} onRate={(_, data) => { problem.level = data.rating as number ; setProblem(problem); }} />
        </Form.Field>
        <Form.Group inline>
          <label>题目类型</label>
          <Form.Radio
            label='提交代码'
            value={1}
            checked={problem.type === 1}
            onChange={(_, data) => { problem.type = data.value as number; setProblem(problem); }}
          />
          <Form.Radio
            label='提交答案'
            value={2}
            checked={problem.type === 2}
            onChange={(_, data) => { problem.type = data.value as number; setProblem(problem); }}
          />
        </Form.Group>
        <Form.Group inline>
          <label>可见性</label>
          <Form.Radio
            label='显示题目'
            checked={!problem.hidden}
            onChange={(_, data) => { problem.hidden = !data.checked; setProblem(problem); }}
          />
          <Form.Radio
            label='隐藏题目'
            checked={problem.hidden}
            onChange={(_, data) => { problem.hidden = data.checked as boolean; setProblem(problem); }}
          />
        </Form.Group>
        <Form.Group inline>
          <label>输入输出类型</label>
          <Form.Radio
            label='标准输入输出'
            checked={problem.config.useStdIO}
            onChange={(_, data) => { problem.config.useStdIO = data.checked as boolean; setProblem(problem); }}
          />
          <Form.Radio
            label='文件输入输出'
            checked={!problem.config.useStdIO}
            onChange={(_, data) => { problem.config.useStdIO = !data.checked; setProblem(problem); }}
          />
        </Form.Group>
        {
          problem.config.useStdIO ? null :
            <Form.Group required inline widths='equal'>
              <Form.Field error={!problem.config.inputFileName}>
                <label>输入文件名</label>
                <Form.Input fluid required defaultValue={problem.config.inputFileName} onChange={e => { problem.config.inputFileName = e.target.value; setProblem(problem); }} />
              </Form.Field>
              <Form.Field error={!problem.config.outputFileName}>
                <label>输出文件名</label>
                <Form.Input fluid required defaultValue={problem.config.outputFileName} onChange={e => { problem.config.outputFileName = e.target.value; setProblem(problem); }} />
              </Form.Field>
            </Form.Group>
        }
      </Form>;

      const AceEditor = require('react-ace').default;
      if (typeof window !== 'undefined' && window) {
        let windowAsAny = window as any;
        windowAsAny.ace.config.set('basePath', '/lib/ace');
      }

      const description = <Grid columns={2} divided>
        <Grid.Row>
          <Grid.Column>
            <div style={{ width: '100%', height: '30em' }}>
              <AceEditor height="100%" width="100%" ref={editor.current} debounceChangePeriod={200} mode="markdown" theme="tomorrow" value={problem.description} onChange={renderPreview}></AceEditor>
            </div>
          </Grid.Column>
          <Grid.Column>
            <div style={{ width: '100%', height: '30em', overflow: 'auto', scrollBehavior: 'auto' }}>
              <MarkdownViewer content={problem.description}></MarkdownViewer>
            </div>
          </Grid.Column>
        </Grid.Row>
      </Grid>;

      const data = <Form>
        {problem.type === 1 ? <>
          <Form.Group widths={16} inline>
            <Form.Button width={1} icon='add' primary onClick={addPoint} />
            <Form.Input width={14} defaultValue={selectedTemplate} placeholder='快速套用模板，格式：输入文件|输出文件|时间限制|内存限制|该点得分，* 代表保持不变' list='templates' onChange={e => setSelectedTemplate(e.target.value)} />
            <datalist id='templates'>
              <option value='${datadir}/${name}${index}.in|${datadir}/${name}${index}.ans|1000|131072|10' />
              <option value='${datadir}/${name}${index}.in|${datadir}/${name}${index}.out|1000|131072|10' />
              <option value='${datadir}/${name}${index0}.in|${datadir}/${name}${index0}.ans|1000|131072|10' />
              <option value='${datadir}/${name}${index0}.in|${datadir}/${name}${index0}.ans|1000|131072|10' />
            </datalist>
            <Form.Button width={1} icon='checkmark' color='green' onClick={applyTemplate} />
          </Form.Group>
          <List relaxed>
            {problem.config.points.map((v, i) =>
              <List.Item key={v.index}>
                <List.Content>
                  <Segment>
                    <Label color='teal' ribbon><span>数据点 #{i + 1}&nbsp;</span><a onClick={removePoint(i)}><Icon name='delete' color='red' /></a></Label>

                    <Form.Group inline widths='equal'>
                      <Form.Field required error={!v.stdInFile}>
                        <label>输入文件</label>
                        <Form.Input fluid required defaultValue={v.stdInFile} onChange={e => { v.stdInFile = e.target.value; setProblem(problem); }} />
                      </Form.Field>
                      <Form.Field required error={!v.stdOutFile}>
                        <label>输出文件</label>
                        <Form.Input fluid required defaultValue={v.stdOutFile} onChange={e => { v.stdOutFile = e.target.value; setProblem(problem); }} />
                      </Form.Field>
                    </Form.Group>

                    <Form.Group inline widths='equal'>
                      <Form.Field required>
                        <label>时间限制</label>
                        <Form.Input fluid required min={0} placeholder='单位：ms' type='number' defaultValue={v.timeLimit.toString()} onChange={e => { v.timeLimit = e.target.valueAsNumber; setProblem(problem); }} />
                      </Form.Field>
                      <Form.Field required>
                        <label>内存限制</label>
                        <Form.Input fluid required min={0} placeholder='单位：kb' type='number' defaultValue={v.memoryLimit.toString()} onChange={e => { v.memoryLimit = e.target.valueAsNumber; setProblem(problem); }} />
                      </Form.Field>
                      <Form.Field required>
                        <label>该点得分</label>
                        <Form.Input fluid required min={0} type='number' defaultValue={v.score.toString()} onChange={e => { v.score = e.target.valueAsNumber; setProblem(problem); }} />
                      </Form.Field>
                    </Form.Group>
                  </Segment>
                </List.Content>
              </List.Item>
            )}
          </List></> :
          <>
            <Form.Field required error={!problem.config.answer.answerFile}>
              <label>答案文件</label>
              <Form.Input required defaultValue={problem.config.answer.answerFile} onChange={e => { problem.config.answer = { ...problem.config.answer, answerFile: e.target.value }; setProblem(problem); }} />
            </Form.Field>
            <Form.Field required>
              <label>该题得分</label>
              <Form.Input required min={0} type='number' defaultValue={problem.config.answer.score.toString()} onChange={e => { problem.config.answer = { ...problem.config.answer, score: e.target.valueAsNumber }; setProblem(problem); }} />
            </Form.Field>
          </>
        }
      </Form>;

      const advanced = <Form>
        <Form.Group inline>
          <label>比较程序</label>
          <Form.Radio
            label='默认'
            checked={!useSpecialJudge}
            onChange={(_, data) => setUseSpecialJudge(!data.checked)}
          />
          <Form.Radio
            label='自定义'
            checked={useSpecialJudge}
            onChange={(_, data) => setUseSpecialJudge(data.checked as boolean)}
          />
        </Form.Group>
        {
          useSpecialJudge ?
            <Form.Field required error={!problem.config.specialJudge}>
              <label>自定义比较程序</label>
              <Form.Input required defaultValue={problem.config.specialJudge} onChange={e => { problem.config.specialJudge = e.target.value; setProblem(problem); }} />
            </Form.Field> :
            <Form.Group inline>
              <label>比较选项</label>
              <Form.Checkbox
                label='忽略行末空格'
                checked={problem.config.comparingOptions.ignoreLineTailWhiteSpaces}
                onChange={(_, data) => { problem.config.comparingOptions.ignoreLineTailWhiteSpaces = data.checked as boolean; setProblem(problem); }}
              />
              <Form.Checkbox
                label='忽略文末空行'
                checked={problem.config.comparingOptions.ignoreTextTailLineFeeds}
                onChange={(_, data) => { problem.config.comparingOptions.ignoreTextTailLineFeeds = data.checked as boolean; setProblem(problem); }}
              />
            </Form.Group>
        }

        {
          problem.type === 1 ?
            <>
              <Form.Field>
                <label>提交语言限制</label>
                <Form.Input placeholder='多个用英文半角分号 ; 分隔，留空为不限' defaultValue={problem.config.languages} onChange={e => { problem.config.languages = e.target.value; setProblem(problem); }} />
              </Form.Field>
              <Form.Field>
                <label>自定义编译参数</label>
                <Form.TextArea placeholder='一行一个，格式：[语言]参数' defaultValue={problem.config.compileArgs} onChange={(_, data) => { problem.config.compileArgs = data.value as string; setProblem(problem); }} />
              </Form.Field>
            </> : null
        }
        <Form.Field>
          <label>附加文件</label>
          <Form.TextArea placeholder='一行一个' defaultValue={problem.config.extraFiles.length === 0 ? '' : problem.config.extraFiles.filter(v => !!v).reduce((accu, next) => `${accu}\n${next}`)} onChange={(_, data) => { problem.config.extraFiles = data.value ? data.value.toString().split('\n') : []; setProblem(problem); }} />
        </Form.Field>
        <Form.Field>
          <label>提交长度限制（单位：byte，0 为不限）</label>
          <Form.Input type='number' defaultValue={problem.config.codeSizeLimit} onChange={(_, data) => { problem.config.codeSizeLimit = parseInt(data.value); setProblem(problem); }} />
        </Form.Field>
      </Form>;

    const utils = <Form>
      <input ref={fileLoader.current} onChange={uploadFile} type='file' accept="application/x-zip-compressed" style={{ filter: 'alpha(opacity=0)', opacity: 0, width: 0, height: 0 }} />
      {
        !processingData ? <Form.Group inline>
          <Form.Button type='button' primary onClick={selectFile}>上传 .zip 数据文件</Form.Button>
          <Form.Button type='button' onClick={viewFileList}>查看文件列表</Form.Button>
          <Form.Button type='button' onClick={downloadFile}>下载数据文件</Form.Button>
          <Form.Button type='button' color='red' onClick={() => setConfirmOpen(true)}>删除数据文件</Form.Button>
        </Form.Group> : <Loader active inline>处理中...</Loader>
      }
    </Form>;

    const submitContent = <Form>
      <Form.Group widths={16} inline>
        <Form.Button width={1} icon='add' primary onClick={addSource} />
      </Form.Group>
      <List relaxed>
        {sources.map((v, i) =>
          <List.Item key={v.index}>
            <List.Content>
              <Segment>
                <Label color='teal' ribbon><span>提交 #{i + 1}&nbsp;</span><a onClick={removeSource(i)}><Icon name='delete' color='red' /></a></Label>
                <Form.Field required error={!v.fileName}>
                  <label>文件名</label>
                  <Form.Input fluid required defaultValue={v.fileName} onChange={e => { sources[i].fileName = e.target.value; setProblem(problem); }} />
                </Form.Field>
              </Segment>
            </List.Content>
          </List.Item>
        )}
      </List>
    </Form>;

    let panes = [
      {
        menuItem: '基本信息', render: () => <Tab.Pane key={0} attached={false}>{basic}</Tab.Pane>
      },
      {
        menuItem: '题目描述', render: () => <Tab.Pane key={1} attached={false}>{description}</Tab.Pane>
      },
      {
        menuItem: '提交内容', render: () => <Tab.Pane key={2} attached={false}>{submitContent}</Tab.Pane>
      },
      {
        menuItem: '题目数据', render: () => <Tab.Pane key={3} attached={false}>{data}</Tab.Pane>
      },
      {
        menuItem: '高级选项', render: () => <Tab.Pane key={4} attached={false}>{advanced}</Tab.Pane>
      }
    ];

    if (problem.id !== 0) panes =
      [
        ...panes,
        {
          menuItem: '实用工具', render: () => <Tab.Pane key={5} attached={false}>{utils}</Tab.Pane>
        }
      ];

    return <>
      <Header as='h2'>题目编辑</Header>
      <Tab menu={{ secondary: true, pointing: true }} panes={panes} />
      <Divider />
      <Button disabled={!canSubmit()} primary onClick={submitChange}>保存</Button>

      <Confirm
        open={confirmOpen}
        cancelButton='取消'
        confirmButton='确定'
        onCancel={() => setConfirmOpen(false)}
        onConfirm={deleteFile}
        content={"删除后不可恢复，确定继续？"}
      />
    </>;
  }

  return render();
};

export default ProblemEdit;