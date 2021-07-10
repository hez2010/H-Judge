import * as React from 'react';
import { GlobalState } from './interfaces/globalState';

const AppContext = React.createContext<GlobalState>({});
export default AppContext;