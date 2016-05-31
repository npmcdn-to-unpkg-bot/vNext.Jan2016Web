import React, {
  Component
}
from 'react'
import Form from "react-jsonschema-form"
import actions from '../redux/actions'

const log = (type) => console.log.bind(console, type);
export default class CommentInput extends React.Component {
  constructor() {
    super();
    this.initializeState();
    this.schema = {
      title: "Comment",
      type: "object",
      required: ["author", "text"],
      properties: {
        author: {
          type: "string",
          title: "Author",
          default: ""
        },
        text: {
          type: "string",
          title: "Comment",
          default: ""
        },
        done: {
          type: "boolean",
          title: "Done?",
          default: false
        }
      }
    };
  }

  initializeState(){
    this.state ={
      commentRecord:{
        author: '',
        text: ''
      }
    }
  }
  validateCommentRecord(){
    var author = this.state.commentRecord.author.trim();
    var text = this.state.commentRecord.text.trim();
    if (!this.state.commentRecord.text || !this.state.commentRecord.author) {
      return null;
    }
    return this.state.commentRecord;
  }
  handleChange(e) {
    let crec = {
      author: e.formData.author,
      text: e.formData.text};

    this.setState({
      commentRecord:crec
    });
  }
  handleSubmit(e) {
    event.preventDefault()

    let commentRecord = this.validateCommentRecord();
    if (!commentRecord) {
      return;
    }
   
    this.props.dispatch(actions.addComment(commentRecord))

    this.initializeState();
  }

  render() {
    return ( < div >
      < Form schema = {
        this.schema
      }
      formData = {
        this.state.commentRecord
      }
      onChange = {
        this.handleChange.bind(this)
      }
      onSubmit = {
        this.handleSubmit.bind(this)
      }
      onError = {
        log("errors")
      } >
      < div >
      < button type = "submit" > Hell Yes Submit < /button> < button > Cancel < /button> < /div> < /Form> < /div>
    );
  }
}