import * as React from 'react';
import { setTitle } from '../../utils/titleHelper';
import { Header, Divider, Feed, Icon, Placeholder } from 'semantic-ui-react';
import { Get } from '../../utils/requestHelper';
import { tryJson } from '../../utils/responseHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { useGlobal } from 'reactn';
import { GlobalState } from '../../interfaces/globalState';
import { getTargetState } from '../../utils/reactnHelper';
import { CommonFuncs } from '../../interfaces/commonFuncs';

interface ActivityModel {
  userId: string,
  userName: string,
  title: string,
  content: string,
  time: Date
}

const Home = () => {
  const [activities, setActivities] = React.useState<ActivityModel[]>([]);
  const [activitiesLoaded, setActivitiesLoaded] = React.useState<boolean>(false);
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));

  React.useEffect(() => {
    setTitle('主页');
    Get('/home/activities')
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs.openPortal(`错误 ${error.errorCode}`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ActivityModel[];
        result = result.map(v => {
          v.time = new Date(v.time.toString());
          return v;
        });
        setActivities(result);
        setActivitiesLoaded(true);
      })
      .catch(err => {
        commonFuncs.openPortal('错误', '动态加载失败', 'red');
      })
  }, []);

  const placeHolder = <Placeholder>
    <Placeholder.Paragraph>
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
    </Placeholder.Paragraph>
  </Placeholder>;

  return (
    <>
      <Header as='h1'>欢迎来到 H::Judge</Header>
      <Divider />
      <Header as='h3'>动态</Header>
      {
        !activitiesLoaded ? placeHolder :
          <Feed>
            {
              activities.length === 0 ? '现在还没有动态哦' : activities.map((v, i) =>
                <Feed.Event key={i}>
                  <Feed.Label>
                    <img src={`/user/${v.userId}`} />
                  </Feed.Label>
                  <Feed.Content>
                    <Feed.Summary>
                      <Feed.User>{v.userName}</Feed.User>{v.title}
                      <Feed.Date>{v.time.toLocaleString(undefined, { hour12: false })}</Feed.Date>
                    </Feed.Summary>
                    <Feed.Extra>{v.content}</Feed.Extra>
                  </Feed.Content>
                </Feed.Event>
              )
            }

          </Feed>
      }
    </>
  );
}

export default Home;