import React, { Component } from 'react'
import CommentInput from './CommentInput'
import CommentList from './CommentList'
import { connect } from 'react-redux'
import {fetchPosts} from '../actions/Posts'
import actions from '../redux/actions'
class App extends Component {

  componentDidMount() {
    this.loadCommentsFromServer();
  }
  
 handleCommentSubmit(comment) {
    var comments = this.state.data;
    // Optimistically set an id on the new comment. It will be replaced by an
    // id generated by the server. In a production application you would likely
    // not use Date.now() for this and would have a more robust system in place.
    comment.id = Date.now();
    var newComments = comments.concat([comment]);
    this.setState({data: newComments});
    $.ajax({
      url: '/api/HelloReactJS/comments',
      dataType: 'json',
      type: 'POST',
      data: comment,
      success: function(data) {
        this.setState({data: data});
      }.bind(this),
      error: function(xhr, status, err) {
        this.setState({data: comments});
        console.error(this.props.url, status, err.toString());
      }.bind(this)
    });
  }

loadCommentsFromServer() {
    $.ajax({
      url: '/api/HelloReactJS/comments',
      dataType: 'json',
      cache: false,
      success: function(data) {
         this.props.dispatch(actions.addComments(data))  
      }.bind(this),
      error: function(xhr, status, err) {
        console.error(this.props.url, status, err.toString());
      }.bind(this)
    });
  }
  render() {
    return (
      <div>
       hi I am the app
       <CommentInput dispatch={this.props.dispatch}/>

       <CommentList comments={this.props.comments}/>
      </div>
    )
  }

}
function mapStateToProps(state){
	return state;
}
export default connect(mapStateToProps)(App)