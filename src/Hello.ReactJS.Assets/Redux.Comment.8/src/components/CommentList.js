import React, { Component } from 'react'

export default class CommentList extends Component {

  render() {
    return (
    <ul>
    {
      this.props.comments.map((comment)=>{
        return (
          <li key={comment.id}> 
            <div className="comment">
                <h2 className="commentAuthor">
                   {comment.author}
                </h2>
                <div>
Hi
                  {comment.text}
                </div>
            
            </div>
          </li>
          )
      })
    }
    </ul>
    )
  }

}
